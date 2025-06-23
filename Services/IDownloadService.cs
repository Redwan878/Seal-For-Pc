using System.Collections.ObjectModel;
using TreaYT.Models;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace TreaYT.Services;

public interface IDownloadService
{
    /// <summary>
    /// Gets the collection of active downloads.
    /// </summary>
    ObservableCollection<DownloadProgress> ActiveDownloads { get; }

    /// <summary>
    /// Gets the collection of completed downloads.
    /// </summary>
    ObservableCollection<DownloadHistoryItem> DownloadHistory { get; }

    /// <summary>
    /// Event raised when a download starts.
    /// </summary>
    event EventHandler<DownloadProgress> DownloadStarted;

    /// <summary>
    /// Event raised when a download's progress changes.
    /// </summary>
    event EventHandler<DownloadProgress> DownloadProgressChanged;

    /// <summary>
    /// Event raised when a download completes.
    /// </summary>
    event EventHandler<DownloadHistoryItem> DownloadCompleted;

    /// <summary>
    /// Event raised when a download fails.
    /// </summary>
    event EventHandler<(string url, string error)> DownloadFailed;

    /// <summary>
    /// Initializes the download service and its dependencies.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Checks if all required dependencies (yt-dlp, FFmpeg) are available.
    /// </summary>
    /// <returns>True if all dependencies are available, false otherwise.</returns>
    Task<bool> CheckDependenciesAsync();

    /// <summary>
    /// Gets information about a video URL.
    /// </summary>
    /// <param name="url">The URL to get information for.</param>
    /// <returns>Video metadata information.</returns>
    Task<VideoData> GetVideoInfoAsync(string url);

    /// <summary>
    /// Gets information about a playlist URL.
    /// </summary>
    /// <param name="url">The URL to get information for.</param>
    /// <returns>Playlist metadata information.</returns>
    Task<PlaylistData> GetPlaylistInfoAsync(string url);

    /// <summary>
    /// Gets available formats for a video URL.
    /// </summary>
    /// <param name="url">The URL to get formats for.</param>
    /// <returns>List of available formats.</returns>
    Task<IEnumerable<FormatData>> GetFormatsAsync(string url);

    /// <summary>
    /// Starts a download with the specified request parameters.
    /// </summary>
    /// <param name="request">The download request parameters.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A tuple containing success status and any error message.</returns>
    Task<(bool success, string? error)> StartDownloadAsync(DownloadRequest request, IProgress<DownloadProgress>? progress = null, CancellationToken ct = default);

    /// <summary>
    /// Pauses a download.
    /// </summary>
    /// <param name="url">The URL of the download to pause.</param>
    /// <returns>True if the download was paused, false otherwise.</returns>
    Task<bool> PauseDownloadAsync(string url);

    /// <summary>
    /// Resumes a paused download.
    /// </summary>
    /// <param name="url">The URL of the download to resume.</param>
    /// <returns>True if the download was resumed, false otherwise.</returns>
    Task<bool> ResumeDownloadAsync(string url);

    /// <summary>
    /// Cancels a download.
    /// </summary>
    /// <param name="url">The URL of the download to cancel.</param>
    /// <returns>True if the download was cancelled, false otherwise.</returns>
    Task<bool> CancelDownloadAsync(string url);

    /// <summary>
    /// Clears the download history.
    /// </summary>
    Task ClearHistoryAsync();

    /// <summary>
    /// Removes a specific item from the download history.
    /// </summary>
    /// <param name="id">The ID of the history item to remove.</param>
    Task RemoveFromHistoryAsync(string id);

    /// <summary>
    /// Gets the download history.
    /// </summary>
    /// <returns>The list of download history items.</returns>
    Task<IEnumerable<DownloadHistoryItem>> GetHistoryAsync();
}

    public DownloadService(ISettingsService settings)
    {
        _settings = settings;
        _ytdl = new YoutubeDL();
        _downloadSemaphore = new SemaphoreSlim(_settings.GetMaxConcurrentDownloads());
        
        ActiveDownloads = new ObservableCollection<DownloadProgress>();
        _history = new List<DownloadProgress>();
        DownloadHistory = new ObservableCollection<DownloadProgress>();
    }

    public async Task InitializeAsync()
    {
        // Set FFmpeg path if configured
        var ffmpegPath = _settings.GetFFmpegPath();
        if (!string.IsNullOrEmpty(ffmpegPath))
        {
            _ytdl.FFmpegPath = ffmpegPath;
        }

        // Try to download/update yt-dlp if needed
        try
        {
            await YoutubeDLSharp.Utils.DownloadYtDlp();
        }
        catch
        {
            // Handle initialization error
        }
    }

    public async Task<bool> CheckDependenciesAsync()
    {
        try
        {
            // Check yt-dlp
            if (!File.Exists(YoutubeDL.DefaultYtDlpPath))
            {
                await YoutubeDLSharp.Utils.DownloadYtDlp();
            }

            // Check FFmpeg
            var ffmpegPath = _settings.GetFFmpegPath();
            if (string.IsNullOrEmpty(ffmpegPath) || !File.Exists(ffmpegPath))
            {
                await YoutubeDLSharp.Utils.DownloadFFmpeg();
                _settings.SetFFmpegPath(YoutubeDL.DefaultFFmpegPath);
                _settings.Save();
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool success, string? error)> DownloadAsync(
        DownloadRequest request,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken ct = default)
    {
        await _downloadSemaphore.WaitAsync(ct);

        try
        {
            var downloadProgress = new DownloadProgress();
            ActiveDownloads.Add(downloadProgress);

            var options = new OptionSet()
            {
                Format = request.Format ?? "bestvideo[height<=1080]+bestaudio/best[height<=1080]",
                ExtractAudio = request.AudioOnly,
                AudioFormat = request.AudioOnly ? AudioConversionFormat.Mp3 : AudioConversionFormat.None,
                EmbedThumbnail = request.EmbedThumbnail,
                WriteInfoJson = request.EmbedMetadata,
                Output = Path.Combine(_settings.GetDownloadPath(), 
                    request.OutputTemplate ?? "%(title)s.%(ext)s"),
                NoPlaylist = string.IsNullOrEmpty(request.Playlist),
                PlaylistStart = request.Playlist?.Split('-').FirstOrDefault(),
                PlaylistEnd = request.Playlist?.Split('-').LastOrDefault()
            };

            var result = await _ytdl.RunVideoDownload(request.Url, ct: ct,
                progress: new Progress<DownloadProgress>(p =>
                {
                    downloadProgress.Title = p.Title;
                    downloadProgress.Progress = p.Progress;
                    downloadProgress.Status = p.Status;
                    downloadProgress.Speed = p.Speed;
                    downloadProgress.ETA = p.ETA;
                    downloadProgress.Size = p.Size;
                    progress?.Report(downloadProgress);
                }));

            downloadProgress.IsCompleted = true;
            _history.Add(downloadProgress);
            UpdateHistory();

            return (result.Success, result.ErrorOutput);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
        finally
        {
            _downloadSemaphore.Release();
        }
    }

    public async Task<IEnumerable<string>> GetFormatsAsync(string url)
    {
        try
        {
            var result = await _ytdl.RunVideoDataFetch(url);
            if (!result.Success) return Array.Empty<string>();

            return result.Data.Formats
                .Where(f => !string.IsNullOrEmpty(f.FormatId))
                .Select(f => $"{f.FormatId} - {f.Format}");
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private void UpdateHistory()
    {
        DownloadHistory.Clear();
        foreach (var item in _history.OrderByDescending(h => h.IsCompleted))
        {
            DownloadHistory.Add(item);
        }
    }
}