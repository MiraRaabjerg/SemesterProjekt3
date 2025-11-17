
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataB;   // <- vigtigt: reference til DataB-projektet

namespace BusinessLogicB
{
    public sealed class ADC : IDisposable
    {
        // Ét sample = tid + glattet værdi
        public sealed record Sample(DateTime T, double V);

        private readonly DSRespiration _sensor;         // Data-lag (hardware)
        private readonly SimpleMovingAverage _sma = new(10);
        private readonly int _tærskel = 10000;          // kan justeres senere
        private double _offset;

        private readonly object _sync = new();
        private readonly List<Sample> _målinger = new();
        private const int MaxSamples = 60 * 60 * 8;     // 8 timer ved ~1 Hz

        public ADC(int dataPin, int clockPin)
        {
            // Opret data-laget
            _sensor = new DSRespiration(dataPin, clockPin);

            // Find hvile-offset
            Console.WriteLine("[ADC] Måler offset (hvile-niveau)...");
            _offset = LæsOffset(15);
            Console.WriteLine($"[ADC] Offset = {_offset:F0}. Klar til måling.");
        }

        private double LæsOffset(int n)
        {
            return _sensor.ReadRawAverage(n);
        }

        
        /// Læs ét glattet sample, gem det i historik og returnér værdien.
        
        public double LæsSignal()
        {
            // Gennemsnit over flere rå læsninger
            double rå = _sensor.ReadRawAverage(5);

            // Fjern hvile-offset
            double offsetKorrigeret = rå - _offset;

            // Glidende gennemsnit
            double glat = _sma.Update(offsetKorrigeret);

            var s = new Sample(DateTime.UtcNow, glat);

            // Trådsikker buffer
            lock (_sync)
            {
                _målinger.Add(s);
                if (_målinger.Count > MaxSamples)
                    _målinger.RemoveAt(0);
            }

            Console.WriteLine($"Måling (glattet): {glat:F0}");
            return glat;
        }

        
        /// Returnerer true hvis alle samples i de sidste 'varighedSekunder' ligger over tærskel.
       
        public bool ErEpisodeIGang(int varighedSekunder) =>
            ErEpisodeIGang(TimeSpan.FromSeconds(varighedSekunder));

        public bool ErEpisodeIGang(TimeSpan varighed)
        {
            DateTime cutoff = DateTime.UtcNow - varighed;

            List<Sample> vindue;
            lock (_sync)
            {
                vindue = _målinger.Where(s => s.T >= cutoff).ToList();
            }

            if (vindue.Count == 0) return false;
            return vindue.All(s => s.V > _tærskel);
        }

        public IReadOnlyList<Sample> HentAlleMålinger()
        {
            lock (_sync) return _målinger.ToList();
        }

        public IReadOnlyList<double> HentAlleVærdier()
        {
            lock (_sync) return _målinger.Select(m => m.V).ToList();
        }

        public void Dispose()
        {
            _sensor.Dispose();
        }
    }

    // Glidende gennemsnit er også business-logic → hører til her
    public sealed class SimpleMovingAverage
    {
        private readonly Queue<double> _q = new();
        private readonly int _n;
        private double _sum;

        public SimpleMovingAverage(int n) => _n = Math.Max(1, n);

        public double Update(double x)
        {
            _q.Enqueue(x);
            _sum += x;
            if (_q.Count > _n) _sum -= _q.Dequeue();
            return _sum / _q.Count;
        }
    }
}
