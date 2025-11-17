using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Iot.Device.Hx711;

namespace DataB
{
    public sealed class DSRespiration : IDisposable
    {

        // Ét sample = tidspunkt + glattet værdi (bedre til grafer)
        public sealed record Sample(DateTime T, double V);

        private readonly Hx711 _hx;
        private readonly SimpleMovingAverage _sma = new(10);   // glatning (glidende gennemsnit) - kan justeres
        private readonly int _tærskel = 10000;                 // justér når I kender niveau
        private double _offset;

        // Trådsikkerhed + buffer
        private readonly object _sync = new();
        private readonly List<Sample> _målinger = new();

        // “ringbuffer”: begræns hvor meget vi gemmer (fx 8 timer ~1 Hz) - kan tilpasses
        private const int MaxSamples = 60 * 60 * 8;

        public DSRespiration(int dataPin, int clockPin)
        {
            // De fleste versioner har denne simple ctor (2 argumenter)
            _hx = new Hx711(dataPin, clockPin);

            // Reset, hvis metoden findes i din version (reflection for at undgå compile-fejl)
            try 
            { 
                _hx.GetType().GetMethod("Reset")?.Invoke(_hx, null); 
            } 
            catch { }

            // “Tare”: mål hvile-gennemsnit og gem som offset
            _offset = ReadAverage(15);
            Console.WriteLine($"[HX] Offset = {_offset:F0}. Klar til måling.");
        }

        public void Dispose()
        {
            try { _hx?.PowerDown(); } catch { }
            try { _hx?.Dispose(); } catch { }
        }

        /// Læser ét glattet sample, gemmer det trådsikkert og returnerer værdien.

        public double LæsSignal()
        {
            // Gennemsnit over flere rå læsninger for stabilitet
            double rå = ReadAverage(5);
            double offsetKorrigeret = rå - _offset;        // fjern hvile-offset 
            double glat = _sma.Update(offsetKorrigeret);   // glidende gennemsnit

            var s = new Sample(DateTime.UtcNow, glat);

            // Trådsikkert append + ringbuffer
            lock (_sync)
            {
                _målinger.Add(s);
                if (_målinger.Count > MaxSamples)
                    _målinger.RemoveAt(0);
            }

            Console.WriteLine($"Måling: {glat:F0}");
            return glat;
        }
        /// Returnerer true hvis alle samples i de sidste 'varighedSekunder' ligger over tærskel.
        /// Tidsbaseret vindue (uafhængig af sampling-frekvens).
  
        public bool ErEpisodeIGang(int varighedSekunder) =>
            ErEpisodeIGang(TimeSpan.FromSeconds(varighedSekunder));

        public bool ErEpisodeIGang(TimeSpan varighed)
        {
            DateTime cutoff = DateTime.UtcNow - varighed;

            List<Sample> vindue;
            lock (_sync)
            {
                // Tag kun de samples der ligger i tidsvinduet
                vindue = _målinger.Where(s => s.T >= cutoff).ToList();
            }

            if (vindue.Count == 0) return false;
            return vindue.All(s => s.V > _tærskel);
        }

        /// <summary>
        /// Returnerer et trådsikkert snapshot af alle samples (tid+værdi).
        /// Bruges til MAUI/graf.
        /// </summary>
        public IReadOnlyList<Sample> HentAlleMålinger()
        {
            lock (_sync) return _målinger.ToList(); // kopi/snapshot
        }

        /// <summary>
        /// (Valgfri) Hvis du kun vil have værdierne til simple beregninger.
        /// </summary>
        public IReadOnlyList<double> HentAlleVærdier()
        {
            lock (_sync) return _målinger.Select(m => m.V).ToList();
        }

        // --- Hjælpere --------------------------------------------------

        private double ReadAverage(int n = 5)
        {
            long sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum += SafeRead();
                Thread.Sleep(2);
            }
            return sum / (double)n;
        }

        /// <summary>
        /// Læs ét rå sample fra HX711, på tværs af versionsforskelle.
        /// Prøver: IsDataReady? → TryRead(out int)? → Read()? → ReadRaw()? 
        /// </summary>
        private int SafeRead()
        {
            // 1) Vent hvis IsDataReady findes
            try
            {
                var isReadyProp = _hx.GetType().GetProperty("IsDataReady");
                if (isReadyProp != null)
                {
                    while (!(bool)isReadyProp.GetValue(_hx, null)!)
                        Thread.SpinWait(50);
                }
            }
            catch { /* ok at ignorere */ }

            // 2) Prøv TryRead(out int)
            var tryRead = _hx.GetType().GetMethod("TryRead", new[] { typeof(int).MakeByRefType() });
            if (tryRead != null)
            {
                object[] args = new object[] { 0 };
                bool ok = (bool)tryRead.Invoke(_hx, args)!;
                if (ok) return (int)args[0];
            }

            // 3) Prøv Read()
            var read = _hx.GetType().GetMethod("Read", Type.EmptyTypes);
            if (read != null)
                return Convert.ToInt32(read.Invoke(_hx, null)!);

            // 4) Prøv ReadRaw()
            var readRaw = _hx.GetType().GetMethod("ReadRaw", Type.EmptyTypes);
            if (readRaw != null)
                return Convert.ToInt32(readRaw.Invoke(_hx, null)!);

            throw new InvalidOperationException("HX711: Ingen kompatibel read-metode fundet i denne version.");
        }
    }

    // Simpelt glidende gennemsnit
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

