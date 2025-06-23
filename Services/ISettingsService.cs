using TreaYT.Models;

namespace TreaYT.Services;

public interface ISettingsService
{
    void Initialize();
    void Save();
    
    // Theme settings
    AppTheme GetTheme();
    void SetTheme(AppTheme theme);
    
    // Download path settings
    string GetDownloadPath();
    void SetDownloadPath(string path);
    
    // FFmpeg settings
    string GetFFmpegPath();
    void SetFFmpegPath(string path);
    
    // Download settings
    int GetMaxConcurrentDownloads();
    void SetMaxConcurrentDownloads(int count);
    
    // Embedding settings
    bool GetEmbedThumbnails();
    void SetEmbedThumbnails(bool embed);
    
    bool GetEmbedMetadata();
    void SetEmbedMetadata(bool embed);
    
    // Subtitle settings
    bool GetWriteSubtitles();
    void SetWriteSubtitles(bool write);
    
    string GetSubtitleLanguage();
    void SetSubtitleLanguage(string language);
    
    bool GetEmbedSubtitles();
    void SetEmbedSubtitles(bool embed);
    
    bool GetWriteAutoSubtitles();
    void SetWriteAutoSubtitles(bool write);
    
    // Format settings
    string GetPreferredVideoFormat();
    void SetPreferredVideoFormat(string format);
    
    string GetPreferredAudioFormat();
    void SetPreferredAudioFormat(string format);
    
    string GetPreferredVideoQuality();
    void SetPreferredVideoQuality(string quality);
    
    // Additional metadata settings
    bool GetWritePlaylistMetadata();
    void SetWritePlaylistMetadata(bool write);
    
    bool GetWriteInfoJson();
    void SetWriteInfoJson(bool write);
    
    bool GetWriteDescription();
    void SetWriteDescription(bool write);
    
    bool GetWriteThumbnail();
    void SetWriteThumbnail(bool write);
    
    // Custom options
    Dictionary<string, string> GetCustomOptions();
    void SetCustomOptions(Dictionary<string, string> options);
}



    public string GetDownloadPath() => _settings.DownloadPath;
    public void SetDownloadPath(string path) => _settings.DownloadPath = path;

    public AppTheme GetTheme() => _settings.Theme;
    public void SetTheme(AppTheme theme) => _settings.Theme = theme;

    public string GetFFmpegPath() => _settings.FFmpegPath;
    public void SetFFmpegPath(string path) => _settings.FFmpegPath = path;

    public int GetMaxConcurrentDownloads() => _settings.MaxConcurrentDownloads;
    public void SetMaxConcurrentDownloads(int count) => _settings.MaxConcurrentDownloads = count;

    public bool GetEmbedThumbnails() => _settings.EmbedThumbnails;
    public void SetEmbedThumbnails(bool embed) => _settings.EmbedThumbnails = embed;

    public bool GetEmbedMetadata() => _settings.EmbedMetadata;
    public void SetEmbedMetadata(bool embed) => _settings.EmbedMetadata = embed;

    public void Save()
    {
        var directory = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_settingsPath, json);
    }

    public void Load()
    {
        if (File.Exists(_settingsPath))
        {
            try
            {
                var json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<Settings>(json);
                if (settings != null)
                {
                    _settings = settings;
                }
            }
            catch
            {
                // Use default settings if file is corrupted
                _settings = new Settings();
            }
        }

        // Ensure download directory exists
        if (!Directory.Exists(_settings.DownloadPath))
        {
            try
            {
                Directory.CreateDirectory(_settings.DownloadPath);
            }
            catch
            {
                // Fallback to default downloads folder if creation fails
                _settings.DownloadPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                    "TreaYT Downloads");
                Directory.CreateDirectory(_settings.DownloadPath);
            }
        }
    }
}