using Microsoft.Maui.Dispatching;
using NightTerrorMaui.PresentationMaui;

namespace NightTerrorMaui.PresentationMaui
{
    public partial class NightPage : ContentPage
    {
        private readonly NightViewModel _vm;

        public NightPage(NightViewModel vm)
        {
            InitializeComponent();

            _vm = vm;
            BindingContext = vm;

            // Når VM siger "Chart ændret", så invaliderer vi grafen
            _vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(NightViewModel.Chart))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ChartView.Invalidate();   // tving GraphicsView til at tegne igen
                    });
                }
            };
        }
    }
}




