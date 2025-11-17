using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NightTerrorMaui.Domain;

namespace NightTerrorMaui.DataMaui
{
    public interface INightRepository
    {
        /// <summary>
        /// Hent nat-data (samples + episoder). Returnér aldrig null.
        /// </summary>
        Task<NightData> GetNightAsync();
    }
}


