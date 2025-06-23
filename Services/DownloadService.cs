using System.Text.Json;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using TreaYT.Models;

namespace TreaYT.Services;

public class DownloadService : IDownloadService
{
    private readonly ISettingsService _settingsService;
    private readonly string _historyFilePath;
    private readonly YoutubeDL _ytdl;
    private readonly Dictionary<string, CancellationTokenSource> _downloadTokens;
    private readonly Dictionary<string, DownloadProgress> _activeDownloads;
    private readonly SemaphoreSlim _downloadSemaphore;
    private readonly object _historyLock = new();

    private string FormatSpeed(double bytesPerSecond)
    {
        string[] units = { "B/s", "KB/s", "MB/s", "GB/s" };
        int unitIndex = 0;
        
        while (bytesPerSecond >= 1024 && unitIndex < units.Length - 1)
        {
            bytesPerSecond /= 1024;
            unitIndex++;
        }
        
        return $"{bytesPerSecond:F2} {units[unitIndex]}";
    }

    public event EventHandler<DownloadProgress> DownloadProgressChanged;
    public event EventHandler<string> DownloadCompleted;
    public event EventHandler<(string downloadId, string error)> DownloadFailed;

    public DownloadService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _historyFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TreaYT",
            "history.json"
        );

        _ytdl = new YoutubeDL();
        _downloadTokens = new Dictionary<string, CancellationTokenSource>();
        _activeDownloads = new Dictionary<string, DownloadProgress>();
        _downloadSemaphore = new SemaphoreSlim(_settingsService.GetMaxConcurrentDownloads(), _settingsService.GetMaxConcurrentDownloads());

        // Ensure history directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(_historyFilePath));
    }

    public async Task InitializeAsync()
    {
        // Update yt-dlp if needed
        await _ytdl.RunUpdate();

        // Configure paths
        _ytdl.YoutubeDLPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TreaYT",
            "yt-dlp.exe"
        );

        _ytdl.FFmpegPath = _settingsService.GetFFmpegPath();
    }

    public async Task CheckDependenciesAsync()
    {
        try
        {
            // Check yt-dlp
            if (!File.Exists(_ytdl.YoutubeDLPath))
            {
                await _ytdl.DownloadYoutubeDL();
            }

            // Check FFmpeg
            if (string.IsNullOrEmpty(_ytdl.FFmpegPath) || !File.Exists(_ytdl.FFmpegPath))
            {
                throw new Exception("FFmpeg path not configured or FFmpeg not found");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to check dependencies: {ex.Message}");
        }
    }

    public async Task<List<string>> GetAvailableFormatsAsync(string url)
    {
        try
        {
            // Configure options to extract all available formats
            var options = new OptionSet()
            {
                ExtractAudio = true,  // Include audio formats
                WriteAutoSub = true,   // Include subtitle info
                ListFormats = true     // List all available formats
            };

            var result = await _ytdl.RunVideoDataFetch(url, options);
            if (!result.Success)
            {
                throw new Exception($"Failed to fetch video formats: {result.ErrorOutput}");
            }

            // Group formats by resolution
            var videoFormats = result.Data.Formats
                .Where(f => !string.IsNullOrEmpty(f.FormatId) && f.Vcodec != "none")
                .GroupBy(f => f.Height ?? 0)
                .OrderByDescending(g => g.Key)
                .Select(g =>
                {
                    var bestFormat = g.OrderByDescending(f => f.Filesize).First();
                    var quality = g.Key > 0 ? $"{g.Key}p" : "Audio Only";
                    var filesize = bestFormat.Filesize > 0 ? $"{bestFormat.Filesize / 1024.0 / 1024.0:F2}MB" : "N/A";
                    var fps = bestFormat.Fps > 0 ? $"{bestFormat.Fps}fps" : "N/A";
                    var codec = !string.IsNullOrEmpty(bestFormat.Vcodec) ? bestFormat.Vcodec : "N/A";

                    return $"{bestFormat.FormatId} - {quality} - {fps} - {codec} - {filesize}";
                })
                .ToList();

            // Add audio-only formats
            var audioFormats = result.Data.Formats
                .Where(f => !string.IsNullOrEmpty(f.FormatId) && f.Vcodec == "none" && f.Acodec != "none")
                .Select(f =>
                {
                    var quality = $"Audio ({f.Acodec})";
                    var filesize = f.Filesize > 0 ? $"{f.Filesize / 1024.0 / 1024.0:F2}MB" : "N/A";
                    var bitrate = f.Abr > 0 ? $"{f.Abr}kbps" : "N/A";

                    return $"{f.FormatId} - {quality} - {bitrate} - {filesize}";
                })
                .ToList();

            var formats = new List<string>();
            formats.AddRange(videoFormats);
            formats.AddRange(audioFormats);
            return formats;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get available formats: {ex.Message}");
        }
    }

    public async Task<string> StartDownloadAsync(DownloadRequest request)
    {
        var downloadId = Guid.NewGuid().ToString();
        var cts = new CancellationTokenSource();
        _downloadTokens[downloadId] = cts;

        try
        {
            await _downloadSemaphore.WaitAsync();

            var progress = new DownloadProgress
            {
                Id = downloadId,
                Url = request.Url,
                Status = DownloadStatus.Initializing,
                DownloadPath = request.DownloadPath,
                Format = request.Format,
                OutputPath = request.DownloadPath
            };
            _activeDownloads[downloadId] = progress;

            // Configure download options
            var options = new OptionSet
            {
                Format = request.Format,
                OutputTemplate = Path.Combine(request.DownloadPath, "%(title)s.%(ext)s"),
                RestrictFilenames = true,
                NoPlaylist = !request.IsPlaylist,
                WriteAutoSub = request.DownloadSubtitles,
                WriteInfoJson = request.SaveMetadata,
                WriteThumbnail = request.SaveThumbnail,
                EmbedThumbnail = request.EmbedThumbnail,
                EmbedMetadata = request.EmbedMetadata,
                ExtractAudio = request.AudioOnly,
                AudioFormat = request.AudioOnly ? request.AudioFormat : null,
                AudioQuality = request.AudioOnly ? request.AudioQuality : null,
                RecodeVideo = request.RecodeVideo ? request.VideoFormat : null,
                SubtitleLanguages = request.SubtitleLanguages,
                DownloadArchive = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TreaYT",
                    "download_archive.txt"
                )
            };

            // Get video info for size estimation
            var videoInfo = await _ytdl.RunVideoDataFetch(request.Url);
            if (!videoInfo.Success)
            {
                throw new Exception($"Failed to fetch video info: {videoInfo.ErrorOutput}");
            }

            var format = videoInfo.Data.Formats.FirstOrDefault(f => f.FormatId == request.Format);
            var totalSize = format?.Filesize ?? 0;
            progress.Size = totalSize.ToString();

            // Setup progress tracking
            _ytdl.DownloadProgressChanged += (sender, e) =>
            {
                progress.Progress = e.Progress;
                progress.Status = "Downloading";

                // Calculate speed and ETA
                if (e.Progress > 0)
                {
                    var elapsedTime = DateTime.Now - progress.StartTime;
                    var downloadedBytes = totalSize * (e.Progress / 100.0);
                    var speedBytesPerSec = downloadedBytes / elapsedTime.TotalSeconds;

                    // Format speed (B/s, KB/s, MB/s)
                    progress.Speed = FormatSpeed(speedBytesPerSec);

                    // Calculate ETA
                    if (speedBytesPerSec > 0)
                    {
                        var remainingBytes = totalSize - downloadedBytes;
                        var remainingSeconds = remainingBytes / speedBytesPerSec;
                        progress.ETA = TimeSpan.FromSeconds(remainingSeconds).ToString(@"hh\:mm\:ss");
                    }
                }

                DownloadProgressChanged?.Invoke(this, progress);
            };

            // Start download process
            var result = request.AudioOnly
                ? await _ytdl.RunAudioDownload(request.Url, options, cts.Token)
                : await _ytdl.RunVideoDownload(request.Url, options, cts.Token);

            if (!result.Success)
            {
                throw new Exception($"Download failed: {result.ErrorOutput}");
            }

            // Update progress
            progress.Status = DownloadStatus.Completed;
            progress.Progress = 100;
            progress.FilePath = result.Data;
            DownloadProgressChanged?.Invoke(this, progress);
            DownloadCompleted?.Invoke(this, downloadId);

            // Add to history
            await AddToHistoryAsync(new DownloadHistoryItem
            {
                Id = downloadId,
                Url = request.Url,
                FilePath = result.Data,
                DownloadDate = DateTime.Now,
                Format = request.Format,
                IsAudioOnly = request.AudioOnly,
                Status = DownloadStatus.Completed
            });

            return downloadId;
        }
        catch (OperationCanceledException)
        {
            var progress = _activeDownloads[downloadId];
            progress.Status = DownloadStatus.Cancelled;
            DownloadProgressChanged?.Invoke(this, progress);
            DownloadFailed?.Invoke(this, (downloadId, "Download cancelled"));
            throw;
        }
        catch (Exception ex)
        {
            var progress = _activeDownloads[downloadId];
            progress.Status = DownloadStatus.Failed;
            progress.Error = ex.Message;
            DownloadProgressChanged?.Invoke(this, progress);
            DownloadFailed?.Invoke(this, (downloadId, ex.Message));
            throw;
        }
        finally
        {
            _downloadSemaphore.Release();
            _downloadTokens.Remove(downloadId);
            _activeDownloads.Remove(downloadId);
        }
    {
        var downloadId = Guid.NewGuid().ToString();
        var cts = new CancellationTokenSource();
        _downloadTokens[downloadId] = cts;

        try
        {
            await _downloadSemaphore.WaitAsync();

            var progress = new DownloadProgress
            {
                Id = downloadId,
                Url = request.Url,
                OutputPath = request.OutputPath,
                Format = request.Format,
                IsAudioOnly = request.AudioOnly
            };

            _activeDownloads[downloadId] = progress;

            // Configure options
            var options = new OptionSet()
            {
                Format = request.Format,
                ExtractAudio = request.AudioOnly,
                AudioFormat = request.PreferredAudioFormat,
                Output = Path.Combine(request.OutputPath, "%(title)s.%(ext)s"),
                EmbedThumbnail = request.EmbedThumbnail,
                WriteInfoJson = request.WriteInfoJson,
                WriteDescription = request.WriteDescription,
                WriteThumbnail = request.WriteThumbnail,
                WriteSubtitles = request.WriteSubtitles,
                SubtitleLanguages = request.SubtitleLanguage,
                EmbedSubtitles = request.EmbedSubtitles,
                WriteAutoSubtitles = request.WriteAutoSubtitles
            };

            // Start download
            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await _ytdl.RunVideoDownload(
                        request.Url,
                        options: options,
                        progress: new Progress<DownloadProgress>(p =>
                        {
                            progress.Progress = p.Progress;
                            progress.Status = p.Status;
                            progress.Speed = p.Speed;
                            progress.ETA = p.ETA;
                            progress.Size = p.Size;
                            DownloadProgressChanged?.Invoke(this, progress);
                        }),
                        ct: cts.Token
                    );

                    if (result.Success)
                    {
                        progress.IsCompleted = true;
                        progress.CompletionTime = DateTime.Now;

                        // Add to history
                        await AddToHistoryAsync(new DownloadHistoryItem
                        {
                            Id = downloadId,
                            Title = result.Data.Title,
                            Url = request.Url,
                            Format = request.Format,
                            OutputPath = request.OutputPath,
                            FileSize = progress.Size,
                            CompletedDate = DateTime.Now,
                            IsAudioOnly = request.AudioOnly,
                            ThumbnailUrl = result.Data.Thumbnail,
                            Duration = TimeSpan.FromSeconds(result.Data.Duration ?? 0),
                            ChannelName = result.Data.Channel,
                            VideoQuality = request.AudioOnly ? string.Empty : request.Format,
                            AudioQuality = request.AudioOnly ? request.PreferredAudioFormat : string.Empty
                        });

                        DownloadCompleted?.Invoke(this, downloadId);
                    }
                    else
                    {
                        progress.HasError = true;
                        progress.ErrorMessage = "Download failed";
                        DownloadFailed?.Invoke(this, (downloadId, "Download failed"));
                    }
                }
                catch (OperationCanceledException)
                {
                    progress.HasError = true;
                    progress.ErrorMessage = "Download cancelled";
                    DownloadFailed?.Invoke(this, (downloadId, "Download cancelled"));
                }
                catch (Exception ex)
                {
                    progress.HasError = true;
                    progress.ErrorMessage = ex.Message;
                    DownloadFailed?.Invoke(this, (downloadId, ex.Message));
                }
                finally
                {
                    _downloadSemaphore.Release();
                    _activeDownloads.Remove(downloadId);
                    _downloadTokens.Remove(downloadId);
                }
            });

            return downloadId;
        }
        catch (Exception ex)
        {
            _downloadSemaphore.Release();
            _downloadTokens.Remove(downloadId);
            throw new Exception($"Failed to start download: {ex.Message}");
        }
    }

    public async Task PauseDownloadAsync(string downloadId)
    {
        if (_activeDownloads.TryGetValue(downloadId, out var progress))
        {
            progress.IsPaused = true;
            // Note: yt-dlp doesn't support true pausing, so we cancel and will need to resume from scratch
            if (_downloadTokens.TryGetValue(downloadId, out var cts))
            {
                cts.Cancel();
            }
        }
    }

    public async Task ResumeDownloadAsync(string downloadId)
    {
        if (_activeDownloads.TryGetValue(downloadId, out var progress))
        {
            progress.IsPaused = false;
            // Re-create the download with the same parameters
            // This is a simplified example - you'd need to store the original request parameters
            var request = new DownloadRequest
            {
                Url = progress.Url,
                Format = progress.Format,
                AudioOnly = progress.IsAudioOnly,
                OutputPath = progress.OutputPath
            };

            await StartDownloadAsync(request);
        }
    }

    public async Task CancelDownloadAsync(string downloadId)
    {
        if (_downloadTokens.TryGetValue(downloadId, out var cts))
        {
            cts.Cancel();
            _downloadTokens.Remove(downloadId);
        }

        if (_activeDownloads.TryGetValue(downloadId, out var progress))
        {
            progress.HasError = true;
            progress.ErrorMessage = "Download cancelled";
            _activeDownloads.Remove(downloadId);
        }
    }

    public async Task<List<DownloadHistoryItem>> GetDownloadHistoryAsync()
    {
        lock (_historyLock)
        {
            if (!File.Exists(_historyFilePath))
            {
                return new List<DownloadHistoryItem>();
            }

            try
            {
                var json = File.ReadAllText(_historyFilePath);
                var history = JsonSerializer.Deserialize<List<DownloadHistoryItem>>(json);
                return history ?? new List<DownloadHistoryItem>();
            }
            catch
            {
                return new List<DownloadHistoryItem>();
            }
        }
    }

    public async Task ClearDownloadHistoryAsync()
    {
        lock (_historyLock)
        {
            File.WriteAllText(_historyFilePath, JsonSerializer.Serialize(new List<DownloadHistoryItem>()));
        }
    }

    public async Task DeleteFromHistoryAsync(string downloadId)
    {
        lock (_historyLock)
        {
            var history = JsonSerializer.Deserialize<List<DownloadHistoryItem>>(File.ReadAllText(_historyFilePath)) ?? new List<DownloadHistoryItem>();
            history.RemoveAll(x => x.Id == downloadId);
            File.WriteAllText(_historyFilePath, JsonSerializer.Serialize(history));
        }
    }

    private async Task AddToHistoryAsync(DownloadHistoryItem item)
    {
        lock (_historyLock)
        {
            var history = new List<DownloadHistoryItem>();
            if (File.Exists(_historyFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_historyFilePath);
                    history = JsonSerializer.Deserialize<List<DownloadHistoryItem>>(json) ?? new List<DownloadHistoryItem>();
                }
                catch { }
            }

            history.Insert(0, item); // Add new items at the start
            File.WriteAllText(_historyFilePath, JsonSerializer.Serialize(history));
        }
    }

    public async Task<string> GetVideoTitleAsync(string url)
    {
        var result = await _ytdl.RunVideoDataFetch(url);
        return result.Success ? result.Data.Title : string.Empty;
    }

    public async Task<string> GetPlaylistTitleAsync(string url)
    {
        var result = await _ytdl.RunVideoDataFetch(url);
        return result.Success ? result.Data.PlaylistTitle : string.Empty;
    }

    public async Task<bool> IsPlaylistAsync(string url)
    {
        var result = await _ytdl.RunVideoDataFetch(url);
        return result.Success && !string.IsNullOrEmpty(result.Data.PlaylistTitle);
    }

    public async Task<int> GetPlaylistItemCountAsync(string url)
    {
        var result = await _ytdl.RunVideoDataFetch(url);
        return result.Success ? result.Data.PlaylistCount ?? 0 : 0;
    }
}