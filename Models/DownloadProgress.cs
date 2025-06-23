namespace TreaYT.Models;

public class DownloadProgress
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public double Progress { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Speed { get; set; } = string.Empty;
    public string ETA { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public bool IsPaused { get; set; }
    public bool IsCompleted { get; set; }
    public bool HasError { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public bool IsAudioOnly { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? CompletionTime { get; set; }

    // Helper methods for status display
    public string FormattedProgress => $"{Progress:F1}%";

    public string FormattedSpeed
    {
        get
        {
            if (string.IsNullOrEmpty(Speed)) return string.Empty;
            return Speed.EndsWith("/s") ? Speed : $"{Speed}/s";
        }
    }

    public string FormattedETA
    {
        get
        {
            if (string.IsNullOrEmpty(ETA)) return string.Empty;
            return ETA.StartsWith("ETA") ? ETA : $"ETA {ETA}";
        }
    }

    public string FormattedSize
    {
        get
        {
            if (string.IsNullOrEmpty(Size)) return string.Empty;
            try
            {
                var size = double.Parse(Size);
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
                return Size;
            }
        }
    }

    public string StatusText
    {
        get
        {
            if (HasError) return $"Error: {ErrorMessage}";
            if (IsCompleted) return "Completed";
            if (IsPaused) return "Paused";
            return Status;
        }
    }

    public TimeSpan ElapsedTime => CompletionTime.HasValue 
        ? CompletionTime.Value - StartTime 
        : DateTime.Now - StartTime;

    public string FormattedElapsedTime => ElapsedTime.ToString(@"hh\:mm\:ss");
}