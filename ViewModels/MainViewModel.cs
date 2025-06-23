using System.Collections.ObjectModel;
using TreaYT.Services;

namespace TreaYT.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IDownloadService _downloadService;
    private readonly ISettingsService _settingsService;
    private string _url = string.Empty;
    private string _selectedFormat = string.Empty;
    private bool _isAnalyzing;
    private bool _isDownloading;
    private bool _audioOnly;
    private string _downloadPath;
    private ObservableCollection<string> _availableFormats;

    public MainViewModel(IDownloadService downloadService, ISettingsService settingsService)
    {
        _downloadService = downloadService;
        _settingsService = settingsService;
        _downloadPath = settingsService.GetDownloadPath();
        _availableFormats = new ObservableCollection<string>();

        AnalyzeCommand = CreateAsyncRelayCommand(AnalyzeUrlAsync, () => !string.IsNullOrWhiteSpace(Url) && !IsAnalyzing);
        DownloadCommand = CreateAsyncRelayCommand(DownloadAsync, () => !string.IsNullOrWhiteSpace(Url) && !IsDownloading);
        BrowseCommand = CreateAsyncRelayCommand(BrowseForDownloadPathAsync);

        ActiveDownloads = _downloadService.ActiveDownloads;
        DownloadHistory = _downloadService.DownloadHistory;
    }

    public string Url
    {
        get => _url;
        set
        {
            if (SetProperty(ref _url, value))
            {
                AnalyzeCommand.RaiseCanExecuteChanged();
                DownloadCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string SelectedFormat
    {
        get => _selectedFormat;
        set => SetProperty(ref _selectedFormat, value);
    }

    public bool IsAnalyzing
    {
        get => _isAnalyzing;
        private set
        {
            if (SetProperty(ref _isAnalyzing, value))
            {
                AnalyzeCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsDownloading
    {
        get => _isDownloading;
        private set
        {
            if (SetProperty(ref _isDownloading, value))
            {
                DownloadCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool AudioOnly
    {
        get => _audioOnly;
        set => SetProperty(ref _audioOnly, value);
    }

    public string DownloadPath
    {
        get => _downloadPath;
        set
        {
            if (SetProperty(ref _downloadPath, value))
            {
                _settingsService.SetDownloadPath(value);
                _settingsService.Save();
            }
        }
    }

    public ObservableCollection<string> AvailableFormats
    {
        get => _availableFormats;
        private set => SetProperty(ref _availableFormats, value);
    }

    public ObservableCollection<DownloadProgress> ActiveDownloads { get; }
    public ObservableCollection<DownloadProgress> DownloadHistory { get; }

    public AsyncRelayCommand AnalyzeCommand { get; }
    public AsyncRelayCommand DownloadCommand { get; }
    public AsyncRelayCommand BrowseCommand { get; }

    private async Task AnalyzeUrlAsync()
    {
        if (string.IsNullOrWhiteSpace(Url)) return;

        try
        {
            IsAnalyzing = true;
            AvailableFormats.Clear();

            var formats = await _downloadService.GetFormatsAsync(Url);
            foreach (var format in formats)
            {
                AvailableFormats.Add(format);
            }

            if (AvailableFormats.Any())
            {
                SelectedFormat = AvailableFormats.First();
            }
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    private async Task DownloadAsync()
    {
        if (string.IsNullOrWhiteSpace(Url)) return;

        try
        {
            IsDownloading = true;

            var request = new DownloadRequest
            {
                Url = Url,
                Format = SelectedFormat.Split(' ').FirstOrDefault(),
                AudioOnly = AudioOnly,
                EmbedThumbnail = _settingsService.GetEmbedThumbnails(),
                EmbedMetadata = _settingsService.GetEmbedMetadata()
            };

            var progress = new Progress<DownloadProgress>();
            await _downloadService.DownloadAsync(request, progress);
        }
        finally
        {
            IsDownloading = false;
        }
    }

    private async Task BrowseForDownloadPathAsync()
    {
        var folderPicker = new Windows.Storage.Pickers.FolderPicker();
        folderPicker.FileTypeFilter.Add("*");

        // Initialize the folder picker with the window handle
        var window = Microsoft.UI.Xaml.Window.Current;
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            DownloadPath = folder.Path;
        }
    }
}