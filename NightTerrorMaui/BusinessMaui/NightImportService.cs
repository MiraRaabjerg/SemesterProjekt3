using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NightTerrorMaui.DataMaui;
using NightTerrorMaui.Domain; // <- namespace for repository-interface

namespace NightTerrorMaui.BusinessMaui
{
    // Denne klasse henter natdata fra data-laget og sikrer at UI altid får gyldige data
    public sealed class NightImportService : INightImportService
    {
        // Repository bruges til at hente rå data (samples + episoder)
        private readonly INightRepository _repo;

        //ctor
        public NightImportService(INightRepository repo)
        {
            _repo = repo;
        }

        public async Task<NightData> ImportAsync()
        {
            // Hent data asynkront fra repository (via. TCP)
            var data = await _repo.GetNightAsync();

            // Defensive defaults . lover altid at levere noget som UI kan arbejde med (også hvis data mangler)
            if (data == null) data = new NightData();
            if (data.Samples == null) data.Samples = new();
            if (data.Episodes == null) data.Episodes = new();

            // Returnér modellen til ViewModel
            return data;
        }
    }
}

