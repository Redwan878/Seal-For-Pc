using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using TreaYT.ViewModels;

namespace TreaYT.Views;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow(IServiceProvider services)
    {
        ViewModel = services.GetRequiredService<MainViewModel>();
        this.InitializeComponent();

        // Set window size and center on screen
        var displayArea = Microsoft.UI.Windowing.DisplayArea.Primary;
        var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

        if (appWindow != null)
        {
            // Set initial size (1280x720)
            var size = new Windows.Graphics.SizeInt32(1280, 720);
            appWindow.Resize(size);

            // Center the window
            if (displayArea != null)
            {
                var centerX = (displayArea.WorkArea.Width - size.Width) / 2;
                var centerY = (displayArea.WorkArea.Height - size.Height) / 2;
                appWindow.Move(new Windows.Graphics.PointInt32(centerX, centerY));
            }

            // Set window title
            Title = "TreaYT Downloader";
            
            // Set window icon (if available)
            // appWindow.SetIcon("Assets/icon.ico");
        }

        // Initialize download service
        _ = InitializeDownloadServiceAsync();
    }

    private async Task InitializeDownloadServiceAsync()
    {
        try
        {
            var downloadService = App.Current.Services.GetRequiredService<IDownloadService>();
            await downloadService.InitializeAsync();
            await downloadService.CheckDependenciesAsync();
        }
        catch (Exception ex)
        {
            // Show error dialog
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Initialization Error",
                Content = $"Failed to initialize download service: {ex.Message}",
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }
    }
}