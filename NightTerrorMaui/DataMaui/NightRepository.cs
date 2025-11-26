using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NightTerrorMaui.Domain;

namespace NightTerrorMaui.DataMaui
{
    // Henter rå tekst via TCP og parser til NightData (samples + episoder + threshold)
    public sealed class NightRepository : INightRepository
    {
        private readonly ITcpNightServer _tcp;
        private readonly string _ip;
        private readonly int _port;

        public NightRepository(ITcpNightServer tcp)
        {
            _tcp = tcp;
            _ip = "192.168.43.229";  // din Pi-IP
            _port = 5000;
        }

        public async Task<NightData> GetNightAsync()
        {
            // 1. Hent rå tekst fra TCP-bæltet
            var raw = await _tcp.GetDataAsync(_ip, _port);

            if (string.IsNullOrWhiteSpace(raw))
                return new NightData();   // tom, men ikke null

            var data = new NightData();

            // 2. Split alle tal ud fra kommaer, linjeskift og mellemrum
            var tokens = raw.Split(new[] { ',', '\n', '\r', ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            var startTime = DateTime.Now;
            double sampleIntervalSeconds = 0.2;   // ca. 5 samples pr sekund – justér om nødvendigt

            // 3. Lav BreathSample for hvert tal
            for (int i = 0; i < tokens.Length; i++)
            {
                if (double.TryParse(tokens[i],
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out double freq))
                {
                    var t = startTime.AddSeconds(i * sampleIntervalSeconds);
                    data.Samples.Add(new BreathSample(t, freq));
                }
            }

            // Hvis ingen samples → tomme episoder og ingen threshold
            if (data.Samples.Count == 0)
            {
                data.Episodes = new List<EpisodeSummary>();
                data.Threshold = null;
                return data;
            }

            // 4. Beregn en threshold ud fra samples (median)
            var orderedFreqs = data.Samples
                .Select(s => s.Frequency)
                .OrderBy(f => f)
                .ToList();

            double threshold = orderedFreqs[orderedFreqs.Count / 2]; // median
            data.Threshold = threshold;

            // 5. Udled episoder ud fra threshold
            data.Episodes = InferEpisodes(data.Samples, threshold);

            return data;
        }

        /// <summary>
        /// Finder episoder hvor frekvensen ligger over threshold i mindst 10 sek.
        /// Returnerer EpisodeSummary med start/slut/duration + estimeret vibrationssekunder.
        /// </summary>
        private static List<EpisodeSummary> InferEpisodes(List<BreathSample> samples, double threshold)
        {
            var result = new List<EpisodeSummary>();
            if (samples.Count == 0) return result;

            var ordered = samples.OrderBy(s => s.Time).ToList();
            bool inEp = false;
            DateTime? start = null;

            // parametre du kan tweake:
            var minDuration = TimeSpan.FromSeconds(10);  // kræv mindst 10 s over tærskel
            var vibPerSec = 0.25;                        // “estimeret” vibrations-andel pr. sekund i en episode

            for (int i = 0; i < ordered.Count; i++)
            {
                var f = ordered[i].Frequency;
                var t = ordered[i].Time;

                if (!inEp && f >= threshold)
                {
                    inEp = true;
                    start = t;
                }
                else if (inEp && f < threshold)
                {
                    var end = t;
                    if (start.HasValue && end - start.Value >= minDuration)
                    {
                        var durSec = (int)Math.Round((end - start.Value).TotalSeconds);
                        var vibSec = (int)Math.Round(durSec * vibPerSec);

                        result.Add(new EpisodeSummary(start.Value, end, durSec, vibSec));
                    }
                    inEp = false;
                    start = null;
                }
            }

            // afslut hvis vi sluttede “inde i” en episode
            if (inEp && start.HasValue)
            {
                var end = ordered[^1].Time;
                if (end > start.Value)
                {
                    var durSec = (int)Math.Round((end - start.Value).TotalSeconds);
                    var vibSec = (int)Math.Round(durSec * 0.25);
                    result.Add(new EpisodeSummary(start.Value, end, durSec, vibSec));
                }
            }

            return result;
        }
    }
}
