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
        // Interface for at hente natdata fra data-laget.
        // Det kan være målinger og episoder fra TCP-forbindelse eller fil.
        // Metoden lover altid at returnere noget – aldrig null.
        Task<NightData> ImportAsync();
    }
}

