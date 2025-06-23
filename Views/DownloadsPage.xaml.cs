using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using TreaYT.ViewModels;

namespace TreaYT.Views;

public sealed partial class DownloadsPage : Page
{
    public DownloadsViewModel ViewModel { get; }

    public DownloadsPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<DownloadsViewModel>();
        this.InitializeComponent();

        // Register for navigation events if needed
        this.Loaded += DownloadsPage_Loaded;
        this.Unloaded += DownloadsPage_Unloaded;
    }

    private void DownloadsPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Initialize any page-specific resources or start monitoring downloads
    }

    private void DownloadsPage_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Clean up any page-specific resources
    }
}