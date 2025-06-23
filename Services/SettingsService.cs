using System.Text.Json;
using TreaYT.Models;

namespace TreaYT.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private AppSettings _settings;
    private readonly object _settingsLock = new();

    public SettingsService()
    {
        _settingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TreaYT",
            "settings.json"
        );

        _settings = new AppSettings();
    }

    public void Initialize()
    {
        lock (_settingsLock)
        {
            try
            {
                // Ensure settings directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath));

                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        _settings = settings;
                    }
                }
                else
                {
                    // Create default settings file
                    Save();
                }
            }
            catch
            {
                // Use default settings if loading fails
                _settings = new AppSettings();
                Save();
            }
        }
    }

    public void Save()
    {
        lock (_settingsLock)
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }
    }

    public AppTheme GetTheme() => _settings.Theme;
    public void SetTheme(AppTheme theme)
    {
        _settings.Theme = theme;
    }

    public string GetDownloadPath() => _settings.DownloadPath;
    public void SetDownloadPath(string path)
    {
        _settings.DownloadPath = path;
    }

    public string GetFFmpegPath() => _settings.FFmpegPath;
    public void SetFFmpegPath(string path)
    {
        _settings.FFmpegPath = path;
    }

    public int GetMaxConcurrentDownloads() => _settings.MaxConcurrentDownloads;
    public void SetMaxConcurrentDownloads(int count)
    {
        _settings.MaxConcurrentDownloads = Math.Max(1, Math.Min(10, count));
    }

    public bool GetEmbedThumbnails() => _settings.EmbedThumbnails;
    public void SetEmbedThumbnails(bool embed)
    {
        _settings.EmbedThumbnails = embed;
    }

    public bool GetEmbedMetadata() => _settings.EmbedMetadata;
    public void SetEmbedMetadata(bool embed)
    {
        _settings.EmbedMetadata = embed;
    }

    public bool GetWriteSubtitles() => _settings.WriteSubtitles;
    public void SetWriteSubtitles(bool write)
    {
        _settings.WriteSubtitles = write;
    }

    public string GetSubtitleLanguage() => _settings.SubtitleLanguage;
    public void SetSubtitleLanguage(string language)
    {
        _settings.SubtitleLanguage = language;
    }

    public bool GetEmbedSubtitles() => _settings.EmbedSubtitles;
    public void SetEmbedSubtitles(bool embed)
    {
        _settings.EmbedSubtitles = embed;
    }

    public bool GetWriteAutoSubtitles() => _settings.WriteAutoSubtitles;
    public void SetWriteAutoSubtitles(bool write)
    {
        _settings.WriteAutoSubtitles = write;
    }

    public string GetPreferredVideoFormat() => _settings.PreferredVideoFormat;
    public void SetPreferredVideoFormat(string format)
    {
        _settings.PreferredVideoFormat = format;
    }

    public string GetPreferredAudioFormat() => _settings.PreferredAudioFormat;
    public void SetPreferredAudioFormat(string format)
    {
        _settings.PreferredAudioFormat = format;
    }

    public string GetPreferredVideoQuality() => _settings.PreferredVideoQuality;
    public void SetPreferredVideoQuality(string quality)
    {
        _settings.PreferredVideoQuality = quality;
    }

    public bool GetWritePlaylistMetadata() => _settings.WritePlaylistMetadata;
    public void SetWritePlaylistMetadata(bool write)
    {
        _settings.WritePlaylistMetadata = write;
    }

    public bool GetWriteInfoJson() => _settings.WriteInfoJson;
    public void SetWriteInfoJson(bool write)
    {
        _settings.WriteInfoJson = write;
    }

    public bool GetWriteDescription() => _settings.WriteDescription;
    public void SetWriteDescription(bool write)
    {
        _settings.WriteDescription = write;
    }

    public bool GetWriteThumbnail() => _settings.WriteThumbnail;
    public void SetWriteThumbnail(bool write)
    {
        _settings.WriteThumbnail = write;
    }

    public Dictionary<string, string> GetCustomOptions() => _settings.CustomOptions;
    public void SetCustomOptions(Dictionary<string, string> options)
    {
        _settings.CustomOptions = options;
    }
}