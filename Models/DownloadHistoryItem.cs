using System.Text.Json.Serialization;

namespace TreaYT.Models;

public class DownloadHistoryItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    [JsonPropertyName("output_path")]
    public string OutputPath { get; set; } = string.Empty;

    [JsonPropertyName("file_size")]
    public string FileSize { get; set; } = string.Empty;

    [JsonPropertyName("completed_date")]
    public DateTime CompletedDate { get; set; } = DateTime.Now;

    [JsonPropertyName("is_audio_only")]
    public bool IsAudioOnly { get; set; }

    [JsonPropertyName("thumbnail_url")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; set; }

    [JsonPropertyName("channel_name")]
    public string ChannelName { get; set; } = string.Empty;

    [JsonPropertyName("video_quality")]
    public string VideoQuality { get; set; } = string.Empty;

    [JsonPropertyName("audio_quality")]
    public string AudioQuality { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

    // Computed properties for UI display
    [JsonIgnore]
    public string FormattedDuration => Duration.ToString(@"hh\:mm\:ss");

    [JsonIgnore]
    public string FormattedCompletedDate => CompletedDate.ToString("g");

    [JsonIgnore]
    public string FormattedFileSize
    {
        get
        {
            if (string.IsNullOrEmpty(FileSize)) return string.Empty;

            try
            {
                var size = double.Parse(FileSize);
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                int order = 0;
                while (size >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    size = size / 1024;
                }
                return $"{size:0.##} {sizes[order]}";
            }
            catch
            {
                return FileSize;
            }
        }
    }

    [JsonIgnore]
    public string QualityInfo
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(VideoQuality)) parts.Add(VideoQuality);
            if (!string.IsNullOrEmpty(AudioQuality)) parts.Add(AudioQuality);
            return string.Join(" | ", parts);
        }
    }
}