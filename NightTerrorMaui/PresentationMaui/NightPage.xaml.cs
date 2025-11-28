namespace NightTerrorMaui.PresentationMaui
{
    public partial class NightPage : ContentPage
    {
        public NightPage(NightViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}



