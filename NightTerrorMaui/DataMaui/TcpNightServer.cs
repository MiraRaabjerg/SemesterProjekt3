using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NightTerrorMaui.DataMaui
{
    public class TcpNightServer : ITcpNightServer
    {
        /// <summary>
        /// Henter rå måledata fra bæltet via TCP.
        /// Sender kommandoen "GET_DATA" og læser hele svaret som tekst.
        /// </summary>
        public async Task<string> GetDataAsync(
            string ip = "raspberrypi.local",
            int port = 5000)
        {
            try
            {
                using var client = new TcpClient();

                // Brug parametrene i stedet for hardkodet IP/port
                await client.ConnectAsync(ip, port);

                using var stream = client.GetStream();
                using var writer = new StreamWriter(stream, Encoding.UTF8)
                {
                    AutoFlush = true
                };
                using var reader = new StreamReader(stream, Encoding.UTF8);

                // Send forespørgsel til bæltet
                await writer.WriteLineAsync("GET_DATA");

                // Læs hele svaret (CSV med værdierne)
                string response = await reader.ReadToEndAsync();

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

