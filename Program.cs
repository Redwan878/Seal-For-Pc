using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using TreaYT.Services;
using TreaYT.ViewModels;
using TreaYT.Views;

namespace TreaYT;

public static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        Microsoft.UI.Xaml.Application.Start((p) =>
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register services
                    services.AddSingleton<IDownloadService, DownloadService>();
                    services.AddSingleton<ISettingsService, SettingsService>();
                    services.AddSingleton<IThemeService, ThemeService>();
                    
                    // Register viewmodels
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<DownloadsViewModel>();
                    services.AddTransient<HistoryViewModel>();
                    services.AddTransient<SettingsViewModel>();
                })
                .Build();

            var app = new App(host);
            app.Run();
        });
    }
}