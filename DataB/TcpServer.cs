using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DataB
{
    public class TcpServer
    {
        private readonly int _port;  // Portnummer som serveren lytter på
        private TcpListener _listener; // TCP listener-objekt

        public TcpServer(int port)
        {
            _port = port; // Gemmer portnummer
        }

        // Starter serveren og accepterer klientforbindelser
        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            Console.WriteLine($"TCP-server startet på port {_port}");

            while (true)
            {
                var client = _listener.AcceptTcpClient();
                Console.WriteLine("Forbindelse oprettet med klient...");
                HandleClient(client); //Kalder HandleClient – metoden er i brug
            }
        }

        // Håndterer en klientforbindelse
        private void HandleClient(TcpClient client)
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            string request = reader.ReadLine();
            Console.WriteLine($"Modtog: {request}");

            if (request == "GET_DATA")
            {
                // Klienten får målingerne fra txt-fil på RPI når man skriver GET_DATA i terminal
                string data = File.ReadAllText("/home/pi/data/respdata.txt");
                writer.WriteLine(data);
                Console.WriteLine("Data sendt til klient.");
            }
            else
            {
                writer.WriteLine("Ukendt kommando");
            }
        }
    }
}

