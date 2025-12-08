using Microsoft.Maui.Dispatching;
using NightTerrorMaui.PresentationMaui;

namespace NightTerrorMaui.PresentationMaui
{
    //Code-behind for NightPage.xaml – styrer binding og grafopdatering
    public partial class NightPage : ContentPage
    {
        private readonly NightViewModel _vm; // ViewModel med data og kommandoer

        public NightPage(NightViewModel vm)
        {
            InitializeComponent(); // Initialiser XAML-indhold

            _vm = vm;
            BindingContext = vm; // Sæt binding til ViewModel

            // Når VM siger "Chart ændret", så opdaterer vi grafen
            _vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(NightViewModel.Chart))
                {
                    // Tving grafen til at tegne igen på UI-tråden
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ChartView.Invalidate();   // genopfrisk ChartView
                    });
                }
            };
        }
    }
}




