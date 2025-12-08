using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NightTerrorMaui.DataMaui
{
    public class TcpNightServer : ITcpNightServer
    {
        // Denne klasse implementerer ITcpNightServer
        // Den opretter forbindelse til bæltet via TCP og henter rå måledata som tekst
        public async Task<string> GetDataAsync(
            string ip = "raspberrypi.local",
            int port = 5000)
        {
            try
            {
                // Opret TCP-klient
                using var client = new TcpClient();

                // Forbind til den angivne IP og port
                await client.ConnectAsync(ip, port);

                // Opret stream til kommunikation
                using var stream = client.GetStream();
                
                // Writer bruges til at sende kommandoer til bæltet
                using var writer = new StreamWriter(stream, Encoding.UTF8)
                {
                    AutoFlush = true
                };
                // Reader bruges til at læse svaret fra bæltet
                using var reader = new StreamReader(stream, Encoding.UTF8);

                // Send forespørgsel til bæltet
                await writer.WriteLineAsync("GET_DATA");

                // Læs hele svaret
                string response = await reader.ReadToEndAsync();
                
                // Fjern unødvendige mellemrum og linjeskift
                return response.Trim();
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"[TCP] Fejl: {ex.Message}");
#endif
                // Ved fejl: returner tom streng så resten af systemet ikke crasher
                return string.Empty;
            }
        }
    }
}

