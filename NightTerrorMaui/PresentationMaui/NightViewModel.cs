using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using NightTerrorMaui.BusinessMaui;
using NightTerrorMaui.Domain;

namespace NightTerrorMaui.PresentationMaui
{
    public class NightViewModel : BindableObject
    {
        private readonly INightImportService _import;
        private readonly IStatsService _stats;

        // Syncfusion graf-data
        public ObservableCollection<SamplePoint> Samples { get; } 
            = new ObservableCollection<SamplePoint>();

        // Episoder
        public ObservableCollection<EpisodeSummary> Episodes { get; } 
            = new ObservableCollection<EpisodeSummary>();

        // Status tekst
        private string _status = "Klar";
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        // KPI: antal episoder
        private int _episodesCount;
        public int EpisodesCount
        {
            get => _episodesCount;
            set { _episodesCount = value; OnPropertyChanged(); }
        }

        // KPI: vibration i sekunder
        private int _totalVibrationSeconds;
        public int TotalVibrationSeconds
        {
            get => _totalVibrationSeconds;
            set { _totalVibrationSeconds = value; OnPropertyChanged(); }
        }

        // Knappens kommando
        public ICommand FetchCommand { get; }

        public NightViewModel(INightImportService import, IStatsService stats)
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

                // Ryd UI
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Samples.Clear();
                    Episodes.Clear();
                });

                // Hent data
                var data = await _import.ImportAsync();
                if (data == null) data = new NightData();
                if (data.Samples == null) data.Samples = new();
                if (data.Episodes == null) data.Episodes = new();

                // Fyld graf-data + episoder
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    for (int i = 0; i < data.Samples.Count; i++)
                    {
                        var s = data.Samples[i]; // BreathSample
                        
                        Samples.Add(new SamplePoint
                        {
                            Index = i,
                            Value = (float)s.Frequency      // ← hvis det hedder Frequency, så ret her!
                        });
                    }

                    foreach (var ep in data.Episodes)
                        Episodes.Add(ep);
                });

                // KPI’er
                var st = _stats.Compute(data);
                EpisodesCount = st.EpisodesCount;
                TotalVibrationSeconds = st.TotalVibrationSeconds;

                Status = $"Modtog {Samples.Count} samples, {Episodes.Count} episoder";
            }
            catch (Exception ex)
            {
                Status = "Fejl under hentning.";
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }

    // Model til Syncfusion graf
    public class SamplePoint
    {
        public int Index { get; set; }
        public float Value { get; set; }
    }
}
