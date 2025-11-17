using System.Net;
using System.Net.Sockets;
using System.Threading;
using DataB;
using BusinessLogicB;

namespace BusinessLogicB
{
    public class Netværk
    {
        private readonly ADC _adc;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;

        public Netværk(ADC adc) => _adc = adc;

        public void StartServer(int port)
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _ = Task.Run(() => AcceptLoop(_cts.Token));
        }

        public void StopServer()
        {
            try
            {
                _cts?.Cancel();
            }
            catch
            {
            }

            try
            {
                _listener?.Stop();
            }
            catch
            {
            }
        }

        private async Task AcceptLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var client = await _listener!.AcceptTcpClientAsync(ct);
                _ = Task.Run(() => HandleClient(client, ct), ct);
            }
        }

        private async Task HandleClient(TcpClient client, CancellationToken ct)
        {
            using var c = client;
            using var stream = c.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            string? line = await reader.ReadLineAsync();
            if (line?.Equals("GET_DATA", StringComparison.OrdinalIgnoreCase) == true)
            {
                var data = _adc.HentAlleMålinger();
                await writer.WriteLineAsync(string.Join(",", data.Select(s => s.V.ToString("F0"))));
            }
            else
            {
                await writer.WriteLineAsync("OK"); // ping/heartbeat
            }
        }
    }
}

