using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLogicB
{
    // Håndterer TCP-kommunikation til MAUI-appen.
    // Lytter på en port (5000) og sender måledata, når der modtages "GET_DATA".
    public sealed class Netværk : IDisposable
    {
        private readonly ADC _adc; //reference til ADC klassen

        private TcpListener? _listener;
        private CancellationTokenSource? _cts; // Bruges til at stoppe serveren

        //Initialiserer netværkslaget med adgang til ADC-data
        public Netværk(ADC adc)
        {
            _adc = adc ?? throw new ArgumentNullException(nameof(adc));
        }

        //Starter TCP-serveren på den angivne port.
        public void StartServer(int port)
        {
            if (_listener != null)
                throw new InvalidOperationException("Serveren er allerede startet.");

            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();

            // Kør accepterings-loop i baggrundstråd
            // (programmet starter en baggrundstråd, som løbende accepterer nye klientforbindelser via TCP – uden at blokere resten af programmet.)
            _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
        }
        
        // Stopper serveren pænt.
        public void StopServer()
        {
            // sikrer at eventuelle fejl ignoreres (fx hvis token allerede er annulleret
            try { _cts?.Cancel(); } catch { /* ignore */ }
            //igen for at undgå at programmet crasher, hvis der sker en fejl (fx hvis den allerede er stoppet.
            try { _listener?.Stop(); } catch { /* ignore */ }
            
            //markerer at serveren er helt stoppet og ressourcerne er frigivet.
            _cts = null;
            _listener = null;
        }

        /// Lytter efter nye klienter, så længe cancellation-token ikke er annulleret.
        private async Task AcceptLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var client = await _listener!
                        .AcceptTcpClientAsync(ct)
                        .ConfigureAwait(false);

                    // Håndter hver klient i sin egen task
                    _ = Task.Run(() => HandleClientAsync(client, ct), ct);
                }
            }
            catch (OperationCanceledException)
            {
                // Forventet når StopServer() kaldes
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Netværk] Fejl i AcceptLoop: {ex.Message}");
            }
        }

        /// Håndterer én klientforbindelse.
        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            using var c = client; //selve klientforbindelsen.
            using var stream = c.GetStream(); //rå dataforbindelse.
            using var reader = new StreamReader(stream); //læser tekst fra klienten.
            using var writer = new StreamWriter(stream) { AutoFlush = true }; //ender tekst til klienten.

            try
            {
                // Læs første linje fra klienten (kommando)
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line is null)
                    return;

                if (line.Equals("GET_DATA", StringComparison.OrdinalIgnoreCase))
                {
                    // Hent alle målinger fra ADC-laget
                    var data = _adc.HentAlleMålinger();

                    // Opretter en CSV-streng (kommasepareret liste) med alle måleværdier fra "data". Altså: 
                    //- henter værdien V og formaterer værdien som heltal med 0 decimaler
                    var csv = string.Join(",", data.Select(s => s.V.ToString("F0")));

                    await writer.WriteLineAsync(csv).ConfigureAwait(false);
                }
                else
                {
                    // Simpelt svar ved ping / ukendt kommando
                    await writer.WriteLineAsync("OK").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Netværk] Fejl ved klient: {ex.Message}");
            }
        }

        //Rydder op og stopper serveren
        public void Dispose()
        {
            StopServer();
            _cts?.Dispose();
        }
    }
}


