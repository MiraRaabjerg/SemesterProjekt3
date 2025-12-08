using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NightTerrorMaui.Domain;

namespace NightTerrorMaui.DataMaui
{
    // Repository der henter og oversætter rå tekstdata til en NightData-model
    // Bruges af NightImportService til at hente målinger og episoder fra TCP-serveren
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

        // Gem tidligere data som old_data
        private string old_data;
        
        
        public async Task<NightData> GetNightAsync()
        {
            // Henter rå tekst fra TCP-bæltet
            var new_data = await _tcp.GetDataAsync(_ip, _port);
            // raw er nyt data
            // TODO: gamle data + nyt data
            var raw = old_data + new_data;
            // TODO: Overskrive det gamle data med det gamle + det nye
            old_data = raw;
            
            if (string.IsNullOrWhiteSpace(raw))
                return new NightData();   // returner tom, men ikke null

            var data = new NightData();

            // Split alle tal ud fra kommaer, linjeskift og mellemrum
            var tokens = raw.Split(new[] { ',', '\n', '\r', ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            var startTime = DateTime.Now; //starttidspunkt for første sample
            double sampleIntervalSeconds = 0.2;   // ca. 5 samples pr sekund

            // Lav BreathSample for hvert tal
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

            // Bestem en tærskel for episode-detektion
            // Her vælger vi bare en fast værdi – justér til fysiologiske krav
            // Note: Denne værdi bor også i bæltets ADC klasse
            double threshold = 1000.0;   // fx 30 respiration pr. minut

            // Beregn episoder ud fra samples og tærskel
            var episodes = InferEpisodes(data.Samples.ToList(), threshold);

            // Gem resultaterne i NightData
            data.Episodes = episodes;
            data.Threshold = threshold;

            return data;
        }

       
        // Finder episoder hvor frekvensen ligger over threshold i mindst 10 sek.
        // Returnerer EpisodeSummary med start/slut/duration + estimeret vibrationssekunder.
        private static List<EpisodeSummary> InferEpisodes(List<BreathSample> samples, double threshold)
        {
            var result = new List<EpisodeSummary>();
            if (samples.Count == 0) return result;

            var ordered = samples.OrderBy(s => s.Time).ToList();
            bool inEp = false;
            DateTime? start = null;

            // parametre der kan justeres:
            var minDuration = TimeSpan.FromSeconds(10);  // kræv mindst 10 s over tærskel
            var vibPerSec = 0.25;                        // Estimeret vibrations-andel pr. sekund i en episode

            for (int i = 0; i < ordered.Count; i++)
            {
                var f = ordered[i].Frequency;
                var t = ordered[i].Time;
                
                //Start en episode hvis frekvensen går over tærskel
                if (!inEp && f >= threshold)
                {
                    inEp = true;
                    start = t;
                }
                // Afslut episode hvis frekvensen falder under tærskel
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

            // Afslut hvis vi stadig er “inde i” en episode ved slutningen
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
