using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NightTerrorMaui.DataMaui
{
    public class TcpNightServer : ITcpNightServer
    {
        public async Task<string> GetDataAsync(string ip = "raspberrypi.local", int port = 5000)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ip, port);

                using var stream = client.GetStream();
                using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                using var reader = new StreamReader(stream, Encoding.UTF8);

                // Send forespørgsel til Pi’en
                await writer.WriteLineAsync("GET_DATA");

                // Læs svar (hele teksten)
                string response = await reader.ReadToEndAsync();

                return response.Trim();
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"TCP-fejl: {ex.Message}");
#endif
                return string.Empty; // Giver blot tomt svar ved fejl i stedet for at crashe app’en
            }
        }
    }
}

