using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataB;   // reference til datalag (DSRespiration)

namespace BusinessLogicB
{
  
    /// Business-logic oven på data-laget:
    /// - glidende gennemsnit
    /// - offset
    /// - tærskel og episode-detektion
    /// - liste over målinger
   
    public sealed class ADC : IDisposable
    {
        // Ét sample = tid + glattet værdi
        public sealed record Sample(DateTime T, double V);

        private readonly DSRespiration _sensor;         // Data-lag (hardware)
        private readonly SimpleMovingAverage _sma = new(10);
        private readonly int _tærskel = 10000;          // kan justeres 
        private double _offset;

        private readonly object _sync = new();
        private readonly List<Sample> _målinger = new();
        private const int MaxSamples = 60 * 60 * 12;     // Max 12 timer ved ~1 Hz

        public ADC(int dataPin, int clockPin)
        {
            // Opret data-laget
            _sensor = new DSRespiration(dataPin, clockPin);

            // Find hvile-offset
            Console.WriteLine("[ADC] Måler offset");
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
            // Gennemsnit af 5 rå samples
            double rå = _sensor.ReadRawAverage(5);

            // Fjern hvile-offset
            double offsetKorrigeret = rå - _offset;

            // Glatning
            double glat = _sma.Update(offsetKorrigeret);

            var s = new Sample(DateTime.UtcNow, glat);

            // Trådsikkerhed, så flere tråde ikke ødelægger listen samtidig.
            lock (_sync)
            {
                //Gem samples i listen
                _målinger.Add(s);
                // Fjern ældste hvis buffer er fuld
                if (_målinger.Count > MaxSamples)
                    _målinger.RemoveAt(0);
            }

            Console.WriteLine($"Måling (glattet): {glat:F0}");
            return glat;
        }

    
        // Returnerer true hvis alle samples i de sidste 'varighedSekunder' ligger over tærskel
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

        //Returnerer en liste af alle samples (altså både tidspunkt T og værdi V)
        //Bruges hvis vi vil have hele historikken med tidspunkter og værdier
        public IReadOnlyList<Sample> HentAlleMålinger()
        {
            lock (_sync) return _målinger.ToList();
        }

        //Returnerer kun værdierne (double) fra alle samples, uden tidspunkter.
        // Bruges hvis vi kun er interesseret i signalets numeriske data
        // det er denne metode vi ville bruge hvis vi laver FFT, amplitudespektrum eller digitalt filter
        // Da vi her kun skal bruge målingernes værdier uden tidspunkt
        public IReadOnlyList<double> HentAlleVærdier()
        {
            lock (_sync) return _målinger.Select(m => m.V).ToList();
        }

        public void Dispose()
        {
            _sensor.Dispose();
        }
    }

    // Glidende gennemsnit over de seneste n værdier
    public sealed class SimpleMovingAverage
    {
        private readonly Queue<double> _q = new();
        private readonly int _n;
        private double _sum;

        public SimpleMovingAverage(int n) => _n = Math.Max(1, n);

        //Opdaterer med ny værdi og returnerer glattet gennemsnit
        public double Update(double x)
        {
            _q.Enqueue(x);
            _sum += x;
            if (_q.Count > _n) _sum -= _q.Dequeue();
            return _sum / _q.Count;
        }
    }
}
