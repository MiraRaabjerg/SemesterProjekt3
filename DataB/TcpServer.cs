using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DataB
{
    public class TcpServer
    {
        private readonly int _port;
        private TcpListener _listener;

        public TcpServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            Console.WriteLine($"TCP-server startet på port {_port}");

            while (true)
            {
                var client = _listener.AcceptTcpClient();
                Console.WriteLine("Forbindelse oprettet med klient...");
                HandleClient(client);
            }
        }

        private void HandleClient(TcpClient client)
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            string request = reader.ReadLine();
            Console.WriteLine($"Modtog: {request}");

            if (request == "GET_DATA")
            {
                // Her kan du hente data fra GemData eller DSRespiration
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

