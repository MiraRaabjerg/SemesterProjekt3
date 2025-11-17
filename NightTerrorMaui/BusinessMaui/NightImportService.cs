using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NightTerrorMaui.DataMaui;
using NightTerrorMaui.Domain; // <- namespace for dit repository-interface

namespace NightTerrorMaui.BusinessMaui
{
    public sealed class NightImportService : INightImportService
    {
        private readonly INightRepository _repo;

        public NightImportService(INightRepository repo)
        {
            _repo = repo;
        }

        public async Task<NightData> ImportAsync()
        {
            // Hent data asynkront via data-laget - kalder repo
            var data = await _repo.GetNightAsync();

            // Defensive defaults . lover altid at levere noget som UI kan arbejde med
            if (data == null) data = new NightData();
            if (data.Samples == null) data.Samples = new();
            if (data.Episodes == null) data.Episodes = new();

            // Returnér modellen til ViewModel
            return data;
        }
    }
}

