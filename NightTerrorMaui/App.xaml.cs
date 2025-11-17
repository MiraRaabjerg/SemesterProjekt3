using NightTerrorMaui.PresentationMaui;

namespace NightTerrorMaui;

public partial class App : Application
{
    public App(NightTerrorMaui.PresentationMaui.NightPage page)
    {
        InitializeComponent();

        // Brug NavigationPage eller bare page, som du vil
        MainPage = new NavigationPage(page);
    }
}

