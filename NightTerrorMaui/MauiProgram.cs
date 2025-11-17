using Microsoft.Extensions.DependencyInjection;   // VIGTIG
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

using NightTerrorMaui.DataMaui;
using NightTerrorMaui.BusinessMaui;
using NightTerrorMaui.PresentationMaui;

namespace NightTerrorMaui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();

            // (valgfrit) log til Output
            builder.Logging.AddDebug();

            // Data
            builder.Services.AddSingleton<INightRepository, NightRepository>(); 

            // Business
            builder.Services.AddSingleton<INightImportService, NightImportService>();
            builder.Services.AddSingleton<IStatsService, StatsService>();

            // Presentation
            builder.Services.AddTransient<NightViewModel>();
            builder.Services.AddTransient<NightPage>();

            //TCP
            builder.Services.AddSingleton<ITcpNightServer, TcpNightServer>();

            // Global exception hooks (hjælper os at se fejl i Output)
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                System.Diagnostics.Debug.WriteLine($"[FATAL] {e.ExceptionObject}");
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"[TASK] {e.Exception}");
                e.SetObserved();
            };

            return builder.Build();
        }
    }
}
