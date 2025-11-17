using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;          
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

        // Metoden vi bruger til at læse råværdier (privat GetNetWeight på Hx711)
        private readonly MethodInfo _getNetWeight;

        public DSRespiration(int dataPin, int clockPin)
        {
            Console.WriteLine("[HX] Opretter Hx711...");

            // Brug den ctor som din version af Hx711 faktisk har:
            _hx = new Hx711(dataPin, clockPin);

            // Sørg for at modulet er tændt/initialiseret
            _hx.PowerUp();

            // Find den private metode GetNetWeight(int numberOfReads)
            _getNetWeight =
                _hx.GetType().GetMethod(
                    "GetNetWeight",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    binder: null,
                    types: new[] { typeof(int) },
                    modifiers: null
                ) ?? throw new InvalidOperationException("Kunne ikke finde GetNetWeight på Hx711.");

            Console.WriteLine("[HX] Måler offset (hvile-niveau)...");

            try
            {
                // “Tare”: mål hvile-gennemsnit og gem som offset
                _offset = ReadAverage(15);
                Console.WriteLine($"[HX] Offset = {_offset:F0}. Klar til måling.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HX] Kunne ikke læse offset: {ex.Message}");
                _offset = 0;   // fortsæt uden offset, så programmet stadig kan køre
            }
        }

        public void Dispose()
        {
            try { _hx?.PowerDown(); } catch { }
            try { _hx?.Dispose(); } catch { }
        }

      
        // Læser ét glattet sample, gemmer det trådsikkert og returnerer værdien.
  
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

      
        // Returnerer true hvis alle samples i de sidste 'varighedSekunder' ligger over tærskel.
        // Tidsbaseret vindue (uafhængig af sampling-frekvens).
      
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
        
        // Returnerer et trådsikkert snapshot af alle samples (tid+værdi).
        // Bruges til MAUI/graf.
        public IReadOnlyList<Sample> HentAlleMålinger()
        {
            lock (_sync) return _målinger.ToList(); // kopi/snapshot
        }


        // (Valgfri) Hvis du kun vil have værdierne til simple beregninger.
        public IReadOnlyList<double> HentAlleVærdier()
        {
            lock (_sync) return _målinger.Select(m => m.V).ToList();
        }

        // Hjælpere

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
        // Læser ét rå sample fra HX711.
        // Vi bruger den private metode GetNetWeight(int numberOfReads) via reflection
        private int SafeRead()
        {
            // Kald GetNetWeight med 1 læsning
            object? result = _getNetWeight.Invoke(_hx, new object[] { 1 });

            // Beskyt mod null og konverter til int
            return Convert.ToInt32(result ?? 0);
        }
    }

    // Simpelt glidende gennemsnit (samme som før)
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


