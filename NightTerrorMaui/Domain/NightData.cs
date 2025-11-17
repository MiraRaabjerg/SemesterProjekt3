using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightTerrorMaui.Domain
{
    public class NightData
    {
        public List<BreathSample> Samples { get; set; }
        public List<EpisodeSummary> Episodes { get; set; }

        // Valgfrit: hvis bæltet sender tærskel, kan vi bruge den i grafen
        public double? Threshold { get; set; }

        public NightData()
        {
            Samples = new List<BreathSample>();
            Episodes = new List<EpisodeSummary>();
        }
    }
}

