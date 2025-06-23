using TreaYT.Services;

namespace TreaYT.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly IThemeService _themeService;
    private AppTheme _selectedTheme;
    private string _ffmpegPath;
    private int _maxConcurrentDownloads;
    private bool _embedThumbnails;
    private bool _embedMetadata;

    public SettingsViewModel(ISettingsService settingsService, IThemeService themeService)
    {
        _settingsService = settingsService;
        _themeService = themeService;

        // Load current settings
        _selectedTheme = _settingsService.GetTheme();
        _ffmpegPath = _settingsService.GetFFmpegPath();
        _maxConcurrentDownloads = _settingsService.GetMaxConcurrentDownloads();
        _embedThumbnails = _settingsService.GetEmbedThumbnails();
        _embedMetadata = _settingsService.GetEmbedMetadata();

        // Commands
        BrowseFFmpegCommand = CreateAsyncRelayCommand(BrowseForFFmpegAsync);
        SaveCommand = CreateAsyncRelayCommand(SaveSettingsAsync);
    }

    public AppTheme SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value))
            {
                _themeService.ApplyTheme(value);
            }
        }
    }

    public string FFmpegPath
    {
        get => _ffmpegPath;
        set => SetProperty(ref _ffmpegPath, value);
    }

    public int MaxConcurrentDownloads
    {
        get => _maxConcurrentDownloads;
        set => SetProperty(ref _maxConcurrentDownloads, Math.Max(1, Math.Min(10, value)));
    }

    public bool EmbedThumbnails
    {
        get => _embedThumbnails;
        set => SetProperty(ref _embedThumbnails, value);
    }

    public bool EmbedMetadata
    {
        get => _embedMetadata;
        set => SetProperty(ref _embedMetadata, value);
    }

    public AsyncRelayCommand BrowseFFmpegCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }

    private async Task BrowseForFFmpegAsync()
    {
        var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
        filePicker.FileTypeFilter.Add(".exe");

        // Initialize the file picker with the window handle
        var window = Microsoft.UI.Xaml.Window.Current;
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

        var file = await filePicker.PickSingleFileAsync();
        if (file != null)
        {
            FFmpegPath = file.Path;
        }
    }

    private async Task SaveSettingsAsync()
    {
        _settingsService.SetTheme(SelectedTheme);
        _settingsService.SetFFmpegPath(FFmpegPath);
        _settingsService.SetMaxConcurrentDownloads(MaxConcurrentDownloads);
        _settingsService.SetEmbedThumbnails(EmbedThumbnails);
        _settingsService.SetEmbedMetadata(EmbedMetadata);
        _settingsService.Save();

        // Show confirmation dialog
        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Settings Saved",
            Content = "Your settings have been saved successfully.",
            CloseButtonText = "OK"
        };

        await dialog.ShowAsync();
    }
}