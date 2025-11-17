using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NightTerrorMaui.Domain;

namespace NightTerrorMaui.BusinessMaui
{
    public record StatsResult(int EpisodesCount, int TotalVibrationSeconds);
    public interface IStatsService
    {
        StatsResult Compute(NightTerrorMaui.Domain.NightData data);
    }
}
