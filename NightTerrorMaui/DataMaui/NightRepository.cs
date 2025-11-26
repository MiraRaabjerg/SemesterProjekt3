using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NightTerrorMaui.Domain;

    namespace NightTerrorMaui.DataMaui
    {
        //henter rå tekst via TCP og parser til NightData.
        public sealed class NightRepository : INightRepository
        {
            private readonly ITcpNightServer _tcp;
            private readonly string _ip;
            private readonly int _port;

            // Du kan vælge at læse IP/port fra settings. Her injicerer vi bare defaults.
            public NightRepository(ITcpNightServer tcp)
            {
                _tcp = tcp;
                _ip = "192.168.43.229";
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

            // 4. Bestem en tærskel for episode-detektion
            //    Her vælger vi bare en fast værdi – justér til jeres fysiologiske krav
            double threshold = 30.0;   // fx 30 "respiration pr. minut" / vilkårlig enhed

            // 5. Beregn episoder ud fra samples og tærskel
            var episodes = InferEpisodes(data.Samples.ToList(), threshold);

            // 6. Gem resultaterne i NightData
            data.Episodes = episodes;
            data.Threshold = threshold;

            return data;
        }

            private static bool TryParseSample(string[] parts, out BreathSample sample)
            {
                sample = default!;
                // S,<time>,<freq>
                if (parts.Length >= 3 && parts[0].Equals("S", StringComparison.OrdinalIgnoreCase))
                {
                    if (TryParseDate(parts[1], out var t) &&
                        double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                    {
                        sample = new BreathSample(t, f);
                        return true;
                    }
                    return false;
                }

                // <time>,<freq>
                if (parts.Length >= 2 &&
                    TryParseDate(parts[0], out var time) &&
                    double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var freq))
                {
                    sample = new BreathSample(time, freq);
                    return true;
                }

                return false;
            }

            private static bool TryParseDate(string s, out DateTime dt)
            {
                // accepter ISO 8601 i både UTC og lokal
                return DateTime.TryParse(s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal, out dt);
            }
            private static List<EpisodeSummary> InferEpisodes(List<BreathSample> samples, double threshold)
            {
                var result = new List<EpisodeSummary>();
                if (samples.Count == 0) return result;

                var ordered = samples.OrderBy(s => s.Time).ToList();
                bool inEp = false;
                DateTime? start = null;

                // parametre du kan tweake:
                var minDuration = TimeSpan.FromSeconds(10);   // kræv mindst 10 s over tærskel
                var vibPerSec = 0.25;                       // “estimeret” vibrations-andel pr. sekund i en episode

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