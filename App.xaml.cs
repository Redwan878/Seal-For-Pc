using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using TreaYT.Services;
using TreaYT.Views;

namespace TreaYT;

public partial class App : Application
{
    private Window? _window;
    private readonly IHost _host;

    public App(IHost host)
    {
        _host = host;
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Initialize services
        var themeService = _host.Services.GetRequiredService<IThemeService>();
        var settingsService = _host.Services.GetRequiredService<ISettingsService>();

        // Create main window
        _window = new MainWindow(_host.Services);

        // Apply theme from settings
        themeService.Initialize(_window);
        themeService.ApplyTheme(settingsService.GetTheme());

        _window.Activate();
    }

    public IServiceProvider Services => _host.Services;
}