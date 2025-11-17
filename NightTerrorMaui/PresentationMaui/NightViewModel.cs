using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;   // MainThread
using Microsoft.Maui.Controls;           // Command
using NightTerrorMaui.BusinessMaui;      // INightImportService, IStatsService
using NightTerrorMaui.Domain;            // NightData, BreathSample, EpisodeSummary

namespace NightTerrorMaui.PresentationMaui
{
    public class NightViewModel : BindableObject
    {
        private readonly INightImportService _import;
        private readonly IStatsService _stats;

        public ObservableCollection<BreathSample> Samples { get; } = new();
        public ObservableCollection<EpisodeSummary> Episodes { get; } = new();

        private string _status = "Klar";
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        private int _episodesCount;
        public int EpisodesCount { get => _episodesCount; set { _episodesCount = value; OnPropertyChanged(); } }

        private int _totalVibrationSeconds;
        public int TotalVibrationSeconds { get => _totalVibrationSeconds; set { _totalVibrationSeconds = value; OnPropertyChanged(); } }

        // Rejses når data og KPI’er er klar (Page lytter på dette for at opdatere grafen)
        public event Action? DataRefreshed;

        public ICommand FetchCommand { get; }

        public NightViewModel(INightImportService import, IStatsService stats) //Konstructor
        {
            _import = import;
            _stats = stats;
            FetchCommand = new Command(async () => await FetchAsync());
        }

        private async Task FetchAsync()
        {
            try
            {
                Status = "Henter...";

                // Ryd på UI-tråden
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Samples.Clear();
                    Episodes.Clear();
                });

                // Hent nat-data (kan køre i baggrunden)
                var data = await _import.ImportAsync();

                // Defensive defaults ift. din model (parameterløs ctor + properties)
                if (data == null)
                    data = new NightData();                           // <- parameterløs ctor
                if (data.Samples == null)
                    data.Samples = new();                             // <- initér property
                if (data.Episodes == null)
                    data.Episodes = new();

                // Fyld collections på UI-tråden
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var s in data.Samples) Samples.Add(s);
                    foreach (var e in data.Episodes) Episodes.Add(e);
                });

                // KPI’er
                var st = _stats.Compute(data);
                EpisodesCount = st.EpisodesCount;
                TotalVibrationSeconds = st.TotalVibrationSeconds;

                Status = $"Modtog {Samples.Count} samples, {Episodes.Count} episoder";

                // Sig til siden at den må opdatere graf/scrollbredde
                await MainThread.InvokeOnMainThreadAsync(() => DataRefreshed?.Invoke());
            }
            catch (Exception ex)
            {
                Status = "Fejl under hentning af data.";
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}

