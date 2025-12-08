using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;   // VIGTIG: // Importerer Dependency Injection (DI)-metoder (AddSingleton, AddTransient osv.), så vi kan registrere services i appen
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Syncfusion.Maui.Core.Hosting;
using NightTerrorMaui.Domain;
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

            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore();   // gør at grafen kan vises

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
                e.SetObserved(); // markerer at fejlen er håndteret
            };

            // Byg og returner den færdige app
            return builder.Build();
        }
    }
}
