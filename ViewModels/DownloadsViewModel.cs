using System.Collections.ObjectModel;
using TreaYT.Services;

namespace TreaYT.ViewModels;

public class DownloadsViewModel : ViewModelBase
{
    private readonly IDownloadService _downloadService;
    private readonly ISettingsService _settingsService;
    private ObservableCollection<DownloadProgress> _activeDownloads;
    private string _url;
    private string _selectedFormat;
    private bool _isAnalyzing;
    private bool _audioOnly;
    private List<string> _availableFormats;

    public DownloadsViewModel(IDownloadService downloadService, ISettingsService settingsService)
    {
        _downloadService = downloadService;
        _settingsService = settingsService;
        _activeDownloads = new ObservableCollection<DownloadProgress>();
        _availableFormats = new List<string>();

        // Initialize commands
        AnalyzeCommand = CreateAsyncRelayCommand(AnalyzeUrlAsync, () => !string.IsNullOrWhiteSpace(Url) && !IsAnalyzing);
        DownloadCommand = CreateAsyncRelayCommand(StartDownloadAsync, CanStartDownload);
        CancelCommand = CreateAsyncRelayCommand<string>(CancelDownloadAsync);
        PauseCommand = CreateAsyncRelayCommand<string>(PauseDownloadAsync);
        ResumeCommand = CreateAsyncRelayCommand<string>(ResumeDownloadAsync);

        // Subscribe to download service events
        _downloadService.DownloadProgressChanged += OnDownloadProgressChanged;
        _downloadService.DownloadCompleted += OnDownloadCompleted;
        _downloadService.DownloadFailed += OnDownloadFailed;
    }

    public ObservableCollection<DownloadProgress> ActiveDownloads
    {
        get => _activeDownloads;
        set => SetProperty(ref _activeDownloads, value);
    }

    public string Url
    {
        get => _url;
        set
        {
            if (SetProperty(ref _url, value))
            {
                AnalyzeCommand.NotifyCanExecuteChanged();
                DownloadCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string SelectedFormat
    {
        get => _selectedFormat;
        set
        {
            if (SetProperty(ref _selectedFormat, value))
            {
                DownloadCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsAnalyzing
    {
        get => _isAnalyzing;
        set
        {
            if (SetProperty(ref _isAnalyzing, value))
            {
                AnalyzeCommand.NotifyCanExecuteChanged();
                DownloadCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool AudioOnly
    {
        get => _audioOnly;
        set => SetProperty(ref _audioOnly, value);
    }

    public List<string> AvailableFormats
    {
        get => _availableFormats;
        set => SetProperty(ref _availableFormats, value);
    }

    public AsyncRelayCommand AnalyzeCommand { get; }
    public AsyncRelayCommand DownloadCommand { get; }
    public AsyncRelayCommand<string> CancelCommand { get; }
    public AsyncRelayCommand<string> PauseCommand { get; }
    public AsyncRelayCommand<string> ResumeCommand { get; }

    private async Task AnalyzeUrlAsync()
    {
        try
        {
            IsAnalyzing = true;
            AvailableFormats = await _downloadService.GetAvailableFormatsAsync(Url);
            if (AvailableFormats.Any())
            {
                SelectedFormat = AvailableFormats.First();
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Analysis Error", $"Failed to analyze URL: {ex.Message}");
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    private bool CanStartDownload()
    {
        return !string.IsNullOrWhiteSpace(Url) && 
               !string.IsNullOrWhiteSpace(SelectedFormat) && 
               !IsAnalyzing;
    }

    private async Task StartDownloadAsync()
    {
        try
        {
            var request = new DownloadRequest
            {
                Url = Url,
                Format = SelectedFormat,
                AudioOnly = AudioOnly,
                OutputPath = _settingsService.GetDownloadPath(),
                EmbedThumbnail = _settingsService.GetEmbedThumbnails(),
                EmbedMetadata = _settingsService.GetEmbedMetadata()
            };

            await _downloadService.StartDownloadAsync(request);
            Url = string.Empty; // Clear URL after starting download
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Download Error", $"Failed to start download: {ex.Message}");
        }
    }

    private async Task CancelDownloadAsync(string downloadId)
    {
        try
        {
            await _downloadService.CancelDownloadAsync(downloadId);
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Cancel Error", $"Failed to cancel download: {ex.Message}");
        }
    }

    private async Task PauseDownloadAsync(string downloadId)
    {
        try
        {
            await _downloadService.PauseDownloadAsync(downloadId);
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Pause Error", $"Failed to pause download: {ex.Message}");
        }
    }

    private async Task ResumeDownloadAsync(string downloadId)
    {
        try
        {
            await _downloadService.ResumeDownloadAsync(downloadId);
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("Resume Error", $"Failed to resume download: {ex.Message}");
        }
    }

    private void OnDownloadProgressChanged(object sender, DownloadProgress progress)
    {
        var existing = ActiveDownloads.FirstOrDefault(d => d.Id == progress.Id);
        if (existing != null)
        {
            var index = ActiveDownloads.IndexOf(existing);
            ActiveDownloads[index] = progress;
        }
        else
        {
            ActiveDownloads.Add(progress);
        }
    }

    private void OnDownloadCompleted(object sender, string downloadId)
    {
        var completed = ActiveDownloads.FirstOrDefault(d => d.Id == downloadId);
        if (completed != null)
        {
            ActiveDownloads.Remove(completed);
        }
    }

    private async void OnDownloadFailed(object sender, (string downloadId, string error) failureInfo)
    {
        var failed = ActiveDownloads.FirstOrDefault(d => d.Id == failureInfo.downloadId);
        if (failed != null)
        {
            ActiveDownloads.Remove(failed);
        }

        await ShowErrorDialogAsync("Download Failed", failureInfo.error);
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