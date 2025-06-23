using System.Text.Json.Serialization;

namespace TreaYT.Models;

public class AppSettings
{
    [JsonPropertyName("theme")]
    public AppTheme Theme { get; set; } = AppTheme.System;

    [JsonPropertyName("download_path")]
    public string DownloadPath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
        "TreaYT Downloads"
    );

    [JsonPropertyName("ffmpeg_path")]
    public string FFmpegPath { get; set; } = string.Empty;

    [JsonPropertyName("max_concurrent_downloads")]
    public int MaxConcurrentDownloads { get; set; } = 3;

    [JsonPropertyName("embed_thumbnails")]
    public bool EmbedThumbnails { get; set; } = true;

    [JsonPropertyName("embed_metadata")]
    public bool EmbedMetadata { get; set; } = true;

    [JsonPropertyName("write_subtitles")]
    public bool WriteSubtitles { get; set; } = false;

    [JsonPropertyName("subtitle_language")]
    public string SubtitleLanguage { get; set; } = "en";

    [JsonPropertyName("embed_subtitles")]
    public bool EmbedSubtitles { get; set; } = false;

    [JsonPropertyName("write_auto_subtitles")]
    public bool WriteAutoSubtitles { get; set; } = false;

    [JsonPropertyName("preferred_video_format")]
    public string PreferredVideoFormat { get; set; } = "mp4";

    [JsonPropertyName("preferred_audio_format")]
    public string PreferredAudioFormat { get; set; } = "mp3";

    [JsonPropertyName("preferred_video_quality")]
    public string PreferredVideoQuality { get; set; } = "1080p";

    [JsonPropertyName("write_playlist_metadata")]
    public bool WritePlaylistMetadata { get; set; } = true;

    [JsonPropertyName("write_info_json")]
    public bool WriteInfoJson { get; set; } = false;

    [JsonPropertyName("write_description")]
    public bool WriteDescription { get; set; } = false;

    [JsonPropertyName("write_thumbnail")]
    public bool WriteThumbnail { get; set; } = false;

    [JsonPropertyName("skip_existing")]
    public bool SkipExisting { get; set; } = true;

    [JsonPropertyName("custom_options")]
    public Dictionary<string, string> CustomOptions { get; set; } = new Dictionary<string, string>();

    public DownloadRequest CreateDownloadRequest(string url, string format, bool audioOnly)
    {
        return new DownloadRequest
        {
            Url = url,
            Format = format,
            AudioOnly = audioOnly,
            OutputPath = DownloadPath,
            EmbedThumbnail = EmbedThumbnails,
            EmbedMetadata = EmbedMetadata,
            WriteSubtitles = WriteSubtitles,
            SubtitleLanguage = SubtitleLanguage,
            EmbedSubtitles = EmbedSubtitles,
            WriteAutoSubtitles = WriteAutoSubtitles,
            PreferredVideoFormat = PreferredVideoFormat,
            PreferredAudioFormat = PreferredAudioFormat,
            PreferredQuality = PreferredVideoQuality,
            WritePlaylist = WritePlaylistMetadata,
            WriteInfoJson = WriteInfoJson,
            WriteDescription = WriteDescription,
            WriteThumbnail = WriteThumbnail,
            SkipExisting = SkipExisting,
            CustomOptions = new Dictionary<string, string>(CustomOptions)
        };
    }
}