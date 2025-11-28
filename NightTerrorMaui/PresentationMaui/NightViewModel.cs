using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using NightTerrorMaui.BusinessMaui;
using NightTerrorMaui.Domain;
using Microsoft.Maui.Graphics;

namespace NightTerrorMaui.PresentationMaui
{
    public class NightViewModel : BindableObject
    {
        private readonly INightImportService _import;
        private readonly IStatsService _stats;

        // Samples som grafen tegner ud fra
        public ObservableCollection<BreathSample> Samples { get; }
            = new ObservableCollection<BreathSample>();

        // Episoder til KPI’er
        public ObservableCollection<EpisodeSummary> Episodes { get; }
            = new ObservableCollection<EpisodeSummary>();

        // 🔸 brugt af SimpleChartDrawable
        public IDrawable Chart { get; }

        // threshold som grafen kan tegne den orange linje ud fra
        public double? Threshold { get; set; }

        private string _status = "Klar";
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        private int _episodesCount;
        public int EpisodesCount
        {
            get => _episodesCount;
            set { _episodesCount = value; OnPropertyChanged(); }
        }

        private int _totalVibrationSeconds;
        public int TotalVibrationSeconds
        {
            get => _totalVibrationSeconds;
            set { _totalVibrationSeconds = value; OnPropertyChanged(); }
        }

        public ICommand FetchCommand { get; }


        public NightViewModel(INightImportService import, IStatsService stats)
        {
            _import = import;
            _stats = stats;
            // vores drawable
            Chart = new SimpleChartDrawable(this);
            FetchCommand = new Command(async () => await FetchAsync());
        }

        private async Task FetchAsync()
        {
            try
            {
                Status = "Henter...";

                // ryd collections på UI-tråden
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Samples.Clear();
                    Episodes.Clear();
                });

                var data = await _import.ImportAsync() ?? new NightData();
                data.Samples ??= new List<BreathSample>();
                data.Episodes ??= new List<EpisodeSummary>();

                // gem threshold til grafen (kan være null)
                Threshold = data.Threshold;

                // fyld samples + episoder
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var s in data.Samples)
                        Samples.Add(s);          // direkte BreathSample

                    foreach (var e in data.Episodes)
                        Episodes.Add(e);
                });

                // KPI’er
                var st = _stats.Compute(data);
                EpisodesCount = st.EpisodesCount;
                TotalVibrationSeconds = st.TotalVibrationSeconds;

                Status = $"Modtog {Samples.Count} samples, {Episodes.Count} episoder";

                // sig til view'et at grafen skal tegnes igen
                OnPropertyChanged(nameof(Chart));
            }
            catch (Exception ex)
            {
                Status = "Fejl under hentning.";
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

    }

    public class SamplePoint
    {
        public int Index { get; set; }
        public float Value { get; set; }
    }
}
