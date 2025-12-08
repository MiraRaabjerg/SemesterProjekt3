using System.Linq;
using NightTerrorMaui.Domain;

namespace NightTerrorMaui.BusinessMaui
{
    // Denne klasse beregner nøgletal ud fra natdata
    // Den opfylder IStatsService og bruges af ViewModel til at vise nøgletal i UI
    public sealed class StatsService : IStatsService
    {
        // Beregner antal episoder og samlet vibrationstid
        public StatsResult Compute(NightData data)
        {
            //Hvis data mangler helt, returnér nøgletal med 0
            // (så UI kan stadig vise tal uden at crashe)
            if (data == null) return new StatsResult(0, 0); 

            int episodes = data.Episodes?.Count ?? 0;
            //summer alle episoders vibrationstid
            int vibSec = data.Episodes?.Sum(e => e.VibrationSeconds) ?? 0;

            // Returnér nøgletallene som StatsResult
            return new StatsResult(episodes, vibSec);
        }
    }
}



