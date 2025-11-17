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
            public NightRepository(ITcpNightServer tcp, string ip = "raspberrypi.local", int port = 5000)
            {
                _tcp = tcp;
                _ip = ip;
                _port = port;
            }

            public async Task<NightData> GetNightAsync()
            {
                var raw = await _tcp.GetDataAsync(_ip, _port);

                if (string.IsNullOrWhiteSpace(raw))
                    return new NightData(); // defensivt: aldrig null

                var samples = new List<BreathSample>();
                var episodes = new List<EpisodeSummary>();
                double? threshold = null;

                var lines = raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length == 0) continue;

                    var parts = trimmed.Split(',', StringSplitOptions.TrimEntries);

                    // --- TÆRSKEL ---
                    if (parts.Length == 2 &&
                        parts[0].Equals("THR", StringComparison.OrdinalIgnoreCase) ||
                        parts[0].Equals("THRESHOLD", StringComparison.OrdinalIgnoreCase) ||
                        parts[0].Equals("threshold", StringComparison.OrdinalIgnoreCase))
                    {
                        if (double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var thr))
                            threshold = thr;
                        continue;
                    }

                    // --- EPISODE-LINJE ---
                    // E,<start>,<end>,<vibrationSeconds>
                    if (parts.Length >= 4 && parts[0].Equals("E", StringComparison.OrdinalIgnoreCase))
                    {
                        if (TryParseDate(parts[1], out var es) &&
                            TryParseDate(parts[2], out var ee) &&
                            int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var vibSec))
                        {

                        episodes.Add(new EpisodeSummary
                        {
                            Start = es,
                            End = ee,
                            DurationSeconds = (int)Math.Round((ee - es).TotalSeconds),
                            VibrationSeconds = vibSec
                        });
                    }
                        continue;
                    }

                    // --- SAMPLE-LINJE ---
                    // <time>,<freq>  ELLER  S,<time>,<freq>
                    if (TryParseSample(parts, out var s))
                    {
                        samples.Add(s);
                        continue;
                    }

                    // Alt andet ignoreres (fx debug-linjer)
                }

                // Hvis ingen episoder kom fra bæltet, kan vi udlede simple episoder ud fra tærskel
                if (episodes.Count == 0 && threshold.HasValue && samples.Count >= 2)
                    episodes = InferEpisodes(samples, threshold.Value);

                return new NightData
                {
                    Samples = samples,
                    Episodes = episodes,
                    Threshold = threshold
                };
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