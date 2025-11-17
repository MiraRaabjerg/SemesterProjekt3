using Microsoft.Maui.ApplicationModel;

namespace NightTerrorMaui.PresentationMaui
{
    public partial class NightPage : ContentPage
    {
        public NightPage(NightViewModel vm) //Konstructor
        {
            InitializeComponent(); // Loader XAML (GraphicView = chart)
            BindingContext = vm; //Kobler View til ViewModel (bindings i XAML)

            Chart.Drawable = new SimpleChartDrawable(vm);

            // Tegn igen når data er klar i VM
            vm.DataRefreshed += () =>
                MainThread.BeginInvokeOnMainThread(() => Chart.Invalidate());

            // Tegn igen ved størrelsesændring
            this.SizeChanged += (_, __) =>
                MainThread.BeginInvokeOnMainThread(() => Chart.Invalidate());
        }
    }
}

