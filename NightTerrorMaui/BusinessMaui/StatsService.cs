using System.Linq;
using NightTerrorMaui.Domain;

namespace NightTerrorMaui.BusinessMaui
{
    public sealed class StatsService : IStatsService
    {
        public StatsResult Compute(NightData data)
        {
            if (data == null) return new StatsResult(0, 0); //hvis der mangler data

            int episodes = data.Episodes?.Count ?? 0;
            int vibSec = data.Episodes?.Sum(e => e.VibrationSeconds) ?? 0;

            return new StatsResult(episodes, vibSec);
        }
    }
}



