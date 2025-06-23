using YoutubeDLSharp.Metadata;

namespace TreaYT.Services;

public interface IVideoInfoService
{
    /// <summary>
    /// Retrieves video information for a given URL.
    /// </summary>
    /// <param name="url">The URL of the video.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the video information.</returns>
    Task<VideoData> GetVideoInfoAsync(string url);

    /// <summary>
    /// Retrieves playlist information for a given URL.
    /// </summary>
    /// <param name="url">The URL of the playlist.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the playlist information.</returns>
    Task<PlaylistData> GetPlaylistInfoAsync(string url);

    /// <summary>
    /// Checks if a given URL is a valid video or playlist URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the URL is valid, false otherwise.</returns>
    bool IsValidUrl(string url);

    /// <summary>
    /// Checks if a given URL is a playlist URL.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <returns>True if the URL is a playlist URL, false otherwise.</returns>
    bool IsPlaylistUrl(string url);

    /// <summary>
    /// Gets the available formats for a video URL.
    /// </summary>
    /// <param name="url">The URL of the video.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of available formats.</returns>
    Task<IEnumerable<FormatData>> GetFormatsAsync(string url);
}