using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using TreaYT.ViewModels;

namespace TreaYT.Views;

public sealed partial class HistoryPage : Page
{
    public HistoryViewModel ViewModel { get; }

    public HistoryPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<HistoryViewModel>();
        this.InitializeComponent();

        // Register for navigation events if needed
        this.Loaded += HistoryPage_Loaded;
        this.Unloaded += HistoryPage_Unloaded;
    }

    private void HistoryPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Refresh history when page is loaded
        _ = ViewModel.RefreshCommand.ExecuteAsync(null);
    }

    private void HistoryPage_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Clean up any page-specific resources
    }
}