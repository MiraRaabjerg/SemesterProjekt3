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
    // ViewModel for NightPage – binder data og kommandoer til UI
    public class NightViewModel : BindableObject
    {
        private readonly INightImportService _import; // Henter NightData fra repository
        private readonly IStatsService _stats; // beregner nøgletal udfra NightData

        // Liste af målinger – vises som kurve i grafen
        public ObservableCollection<BreathSample> Samples { get; }
            = new ObservableCollection<BreathSample>();

        // Liste af episoder – vises som nøgletal og evt. i liste
        public ObservableCollection<EpisodeSummary> Episodes { get; }
            = new ObservableCollection<EpisodeSummary>();

        // Tegneobjekt til grafen – opdateres når Samples ændres
        public IDrawable Chart { get; }

        // Tærskelværdi – bruges til at tegne orange linje i grafen
        public double? Threshold { get; set; }

        // Statusbesked
        private string _status = "Klar";
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        // Antal episoder – vises som nøgletal
        private int _episodesCount;
        public int EpisodesCount
        {
            get => _episodesCount;
            set { _episodesCount = value; OnPropertyChanged(); }
        }

        // Samlet vibrationstid – vises som nøgletal
        private int _totalVibrationSeconds;
        public int TotalVibrationSeconds
        {
            get => _totalVibrationSeconds;
            set { _totalVibrationSeconds = value; OnPropertyChanged(); }
        }

        // Kommando til at hente data – bindes til knappen
        public ICommand FetchCommand { get; }


        // Constructor – modtager services og opretter kommando + graf
        public NightViewModel(INightImportService import, IStatsService stats)
        {
            _import = import;
            _stats = stats;
            // vores drawable
            Chart = new SimpleChartDrawable(this); // grafen tegnes ud fra Samples
            FetchCommand = new Command(async () => await FetchAsync());
        }

        // henter og opdaterer data
        private async Task FetchAsync()
        {
            try
            {
                Status = "Henter...";

                // Ryd tidligere data på UI-tråden
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Samples.Clear();
                    Episodes.Clear();
                });

                // Hent NightData fra importservice
                var data = await _import.ImportAsync() ?? new NightData();
                data.Samples ??= new List<BreathSample>();
                data.Episodes ??= new List<EpisodeSummary>();

                // gem tærskel til grafen (kan være null)
                Threshold = data.Threshold;

                // fyld samples + episoder
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var s in data.Samples)
                        Samples.Add(s);          // direkte BreathSample

                    foreach (var e in data.Episodes)
                        Episodes.Add(e);
                });

                // Beregn nøgletal og opdater felter
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

    // Hjælpeklasse til graftegning – bruges af SimpleChartDrawable
    public class SamplePoint
    {
        public int Index { get; set; }
        public float Value { get; set; }
    }
}
