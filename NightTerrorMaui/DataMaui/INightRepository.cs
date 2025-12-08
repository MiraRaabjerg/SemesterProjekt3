using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NightTerrorMaui.Domain;

namespace NightTerrorMaui.DataMaui
{
    //Interface for at hente natdata fra bæltet eller fil.
    public interface INightRepository
    {
        // Henter natdata (målinger og episoder) som NightData
        // Bruges af NightImportService til at hente og videresende data til ViewModel
        Task<NightData> GetNightAsync();
    }
}


