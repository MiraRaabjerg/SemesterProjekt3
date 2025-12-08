using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightTerrorMaui.Domain
{
    // EpisodeSummary repræsenterer én episode hvor frekvensen har været over tærskel
    // Bruges til at vise nøgletal og beregne samlet vibrations- og varighedstid
    public class EpisodeSummary
    {
        // Starttidspunkt for episoden
        public DateTime Start { get; set; }
        
        // Sluttidspunkt for episoden
        public DateTime End { get; set; }

        // Bekvemmelighedsfelter til visning
        public int DurationSeconds { get; set; }   // (End-Start).TotalSeconds afrundet
        public int VibrationSeconds { get; set; }   // summeret vibrationstid inden for episoden

        // Tom ctor – bruges ikke
        public EpisodeSummary() { }

        // Constructor med alle felter – bruges ved beregning i InferEpisodes() i NightRepository
        public EpisodeSummary(DateTime start, DateTime end, int durationSeconds, int vibrationSeconds)
        {
            Start = start;
            End = end;
            DurationSeconds = durationSeconds;
            VibrationSeconds = vibrationSeconds;
        }
    }
}

