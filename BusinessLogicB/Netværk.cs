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
    // Lytter på en port og sender måledata, når der modtages "GET_DATA".
    public sealed class Netværk : IDisposable
    {
        private readonly ADC _adc;

        private TcpListener? _listener;
        private CancellationTokenSource? _cts;

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
            _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
        }

        // Stopper serveren pænt.
        public void StopServer()
        {
            try { _cts?.Cancel(); } catch { /* ignore */ }
            try { _listener?.Stop(); } catch { /* ignore */ }

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
            using var c = client;
            using var stream = c.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            try
            {
                // Læs første linje fra klienten (kommando)
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line is null)
                    return;

                if (line.Equals("GET_DATA", StringComparison.OrdinalIgnoreCase))
                {
                    // Hent alle målinger fra ADC-laget
                    // Ret HentAlleMålinger hvis din metode hedder noget andet
                    var data = _adc.HentAlleMålinger();

                    // Lav CSV-streng, fx: 1234,1235,1236,...
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

        public void Dispose()
        {
            StopServer();
            _cts?.Dispose();
        }
    }
}


