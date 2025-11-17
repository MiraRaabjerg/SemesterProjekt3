using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NightTerrorMaui.Domain;

namespace NightTerrorMaui.BusinessMaui
{
    public interface INightImportService
    {
        /// Henter NightData (samples + episoder) fra data-laget (TCP/fil mm.).
        /// Returnerer aldrig null – i værste fald en tom NightData().
        Task<NightData> ImportAsync();
    }
}

