using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightTerrorMaui.Domain
{
    public class EpisodeSummary
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        // Bekvemmelighedsfelter til visning
        public int DurationSeconds { get; set; }   // (End-Start).TotalSeconds afrundet
        public int VibrationSeconds { get; set; }   // summeret vibrationstid inden for episoden

        public EpisodeSummary() { }

        public EpisodeSummary(DateTime start, DateTime end, int durationSeconds, int vibrationSeconds)
        {
            Start = start;
            End = end;
            DurationSeconds = durationSeconds;
            VibrationSeconds = vibrationSeconds;
        }
    }
}

