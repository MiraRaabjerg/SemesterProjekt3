using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightTerrorMaui.Domain
{
    // NightData repræsenterer alle data fra én nat
    // Indeholder både målinger og beregnede episoder
    public class NightData
    {
        // Liste af alle målinger (tid + frekvens)
        public List<BreathSample> Samples { get; set; }
        
        // Liste af alle episoder over tærskel (start/slut/duration/vibration)
        public List<EpisodeSummary> Episodes { get; set; }

        // Valgfrit: hvis bæltet sender tærskel, kan vi bruge den i grafen
        public double? Threshold { get; set; }

        //initialiserer tomme lister så UI aldrig får null
        public NightData()
        {
            Samples = new List<BreathSample>();
            Episodes = new List<EpisodeSummary>();
        }
    }
}

