using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NightTerrorMaui.Domain;

namespace NightTerrorMaui.BusinessMaui
{
    // StatsResult er en lille datastruktur der indeholder:
    // Antal episoder fundet i natten
    // Samlet estimeret vibrationstid i sekunder
    public record StatsResult(int EpisodesCount, int TotalVibrationSeconds);
    public interface IStatsService
    {
       // IStatsService er et interface for statistikberegning.
       // Det tager et NightData-objekt og udregner KPI'er (nøgletal) til visning.
        StatsResult Compute(NightTerrorMaui.Domain.NightData data);
    }
}
