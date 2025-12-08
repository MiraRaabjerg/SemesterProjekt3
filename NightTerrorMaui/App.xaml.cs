using NightTerrorMaui.PresentationMaui;
using NightTerrorMaui.BusinessMaui;
using NightTerrorMaui.DataMaui;

namespace NightTerrorMaui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Byg hele kæden manuelt (bælte → repository → services → ViewModel → side)

        var tcp = new TcpNightServer();          // Data-laget: TCP-klient
        var repo = new NightRepository(tcp);      // Data-laget: parser tekst → NightData
        var imp = new NightImportService(repo);  // Business-lag: importerer NightData
        var stats = new StatsService();            // Business-lag: beregner KPI’er
        var vm = new NightViewModel(imp, stats);// ViewModel til NightPage
        var page = new NightPage(vm);             // View

        // Evt. med navigation - giver mulighed for at tilføje flere sider senere
        MainPage = new NavigationPage(page);
        // ellers uden navigation:
        // MainPage = page;
    }
}


