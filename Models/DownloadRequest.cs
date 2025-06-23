namespace TreaYT.Models;

public class DownloadRequest
{
    public string Url { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public bool AudioOnly { get; set; }
    public string OutputPath { get; set; } = string.Empty;
    public bool EmbedThumbnail { get; set; }
    public bool EmbedMetadata { get; set; }
    public bool ExtractAudio { get; set; }
    public string PreferredAudioFormat { get; set; } = "mp3";
    public string PreferredVideoFormat { get; set; } = "mp4";
    public string PreferredQuality { get; set; } = "best";
    public string PreferredResolution { get; set; } = "1080p";
    public bool UsePreferredResolution { get; set; } = false;
    public bool SkipExisting { get; set; } = true;
    public bool WriteSubtitles { get; set; }
    public string SubtitleLanguage { get; set; } = "en";
    public bool EmbedSubtitles { get; set; }
    public bool WriteAutoSubtitles { get; set; }
    public bool WritePlaylist { get; set; }
    public bool WriteInfoJson { get; set; }
    public bool WriteDescription { get; set; }
    public bool WriteThumbnail { get; set; }
    public Dictionary<string, string> CustomOptions { get; set; } = new Dictionary<string, string>();

    public string GetOutputTemplate()
    {
        // Default output template for single videos
        var template = "%(title)s.%(ext)s";

        // If it's a playlist, include playlist information in the filename
        if (WritePlaylist)
        {
            template = "%(playlist_title)s/%(playlist_index)s - %(title)s.%(ext)s";
        }

        return template;
    }

    public List<string> BuildYtDlpArguments()
    {
        var args = new List<string>
        {
            // Basic options
            "--no-warnings",
            "--no-color",
            "--progress",
            "--newline",

            // Format selection
            // Format selection based on preferences
            AudioOnly ? "-x" : "-f",
            AudioOnly
                ? "--audio-format"
                : UsePreferredResolution
                    ? $"bestvideo[height<={PreferredResolution.TrimEnd('p')}]+bestaudio/best[height<={PreferredResolution.TrimEnd('p')}]/best"
                    : Format,
            AudioOnly ? PreferredAudioFormat : Format,

            // Output template
            "-o",
            Path.Combine(OutputPath, GetOutputTemplate())
        };

        // Thumbnail embedding
        if (EmbedThumbnail)
        {
            args.Add("--embed-thumbnail");
        }

        // Metadata embedding
        if (EmbedMetadata)
        {
            args.Add("--embed-metadata");
            args.Add("--add-metadata");
        }

        // Subtitle options
        if (WriteSubtitles)
        {
            args.Add("--write-sub");
            args.Add("--sub-lang");
            args.Add(SubtitleLanguage);

            if (EmbedSubtitles)
            {
                args.Add("--embed-subs");
            }
        }

        if (WriteAutoSubtitles)
        {
            args.Add("--write-auto-sub");
        }

        // Additional metadata
        if (WriteInfoJson)
        {
            args.Add("--write-info-json");
        }

        if (WriteDescription)
        {
            args.Add("--write-description");
        }

        if (WriteThumbnail)
        {
            args.Add("--write-thumbnail");
        }

        // Skip existing files
        if (SkipExisting)
        {
            args.Add("--no-overwrites");
        }

        // Add custom options
        foreach (var option in CustomOptions)
        {
            args.Add(option.Key);
            if (!string.IsNullOrEmpty(option.Value))
            {
                args.Add(option.Value);
            }
        }

        // Add the URL last
        args.Add(Url);

        return args;
    }
}