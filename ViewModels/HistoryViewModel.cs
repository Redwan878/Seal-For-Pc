using System.Collections.ObjectModel;
using TreaYT.Services;

namespace TreaYT.ViewModels;

public class HistoryViewModel : ViewModelBase
{
    private readonly IDownloadService _downloadService;
    private ObservableCollection<DownloadHistoryItem> _historyItems;
    private string _searchQuery;
    private DownloadHistoryItem _selectedItem;

    public HistoryViewModel(IDownloadService downloadService)
    {
        _downloadService = downloadService;
        _historyItems = new ObservableCollection<DownloadHistoryItem>();

        // Initialize commands
        ClearHistoryCommand = CreateAsyncRelayCommand(ClearHistoryAsync);
        DeleteItemCommand = CreateAsyncRelayCommand<DownloadHistoryItem>(DeleteHistoryItemAsync);
        OpenFolderCommand = CreateAsyncRelayCommand<string>(OpenFolderAsync);
        RefreshCommand = CreateAsyncRelayCommand(LoadHistoryAsync);

        // Load initial history
        _ = LoadHistoryAsync();

        // Subscribe to download service events
        _downloadService.DownloadCompleted += OnDownloadCompleted;
    }

    public ObservableCollection<DownloadHistoryItem> HistoryItems
    {
        get => _historyItems;
        set => SetProperty(ref _historyItems, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                _ = FilterHistoryAsync();
            }
        }
    }

    public DownloadHistoryItem SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public AsyncRelayCommand ClearHistoryCommand { get; }
    public AsyncRelayCommand<DownloadHistoryItem> DeleteItemCommand { get; }
    public AsyncRelayCommand<string> OpenFolderCommand { get; }
    public AsyncRelayCommand RefreshCommand { get; }

    private async Task LoadHistoryAsync()
    {
        try
        {
            var history = await _downloadService.GetDownloadHistoryAsync();
            HistoryItems = new ObservableCollection<DownloadHistoryItem>(history);
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("History Error", $"Failed to load download history: {ex.Message}");
        }
    }

    private async Task FilterHistoryAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadHistoryAsync();
            return;
        }

        try
        {
            var history = await _downloadService.GetDownloadHistoryAsync();
            var filtered = history.Where(item =>
                item.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                item.Url.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

            HistoryItems = new ObservableCollection<DownloadHistoryItem>(filtered);
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Filter Error", $"Failed to filter history: {ex.Message}");
        }
    }

    private async Task ClearHistoryAsync()
    {
        try
        {
            // Show confirmation dialog
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Clear History",
                Content = "Are you sure you want to clear all download history? This action cannot be undone.",
                PrimaryButtonText = "Clear",
                CloseButtonText = "Cancel",
                DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close
            };

            var result = await dialog.ShowAsync();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                await _downloadService.ClearDownloadHistoryAsync();
                HistoryItems.Clear();
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Clear History Error", $"Failed to clear history: {ex.Message}");
        }
    }

    private async Task DeleteHistoryItemAsync(DownloadHistoryItem item)
    {
        if (item == null) return;

        try
        {
            await _downloadService.DeleteFromHistoryAsync(item.Id);
            HistoryItems.Remove(item);
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Delete Error", $"Failed to delete history item: {ex.Message}");
        }
    }

    private async Task OpenFolderAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        try
        {
            // Use Windows.System.Launcher to open the folder
            var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(path);
            await Windows.System.Launcher.LaunchFolderAsync(folder);
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Open Folder Error", $"Failed to open folder: {ex.Message}");
        }
    }

    private void OnDownloadCompleted(object sender, string downloadId)
    {
        // Refresh history when a download completes
        _ = LoadHistoryAsync();
    }

    private async Task ShowErrorDialogAsync(string title, string message)
    {
        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };

        await dialog.ShowAsync();
    }
}