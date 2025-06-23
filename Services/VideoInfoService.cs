using System.Text.RegularExpressions;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace TreaYT.Services;

public class VideoInfoService : IVideoInfoService
{
    private readonly YoutubeDL _youtubeDL;
    private static readonly Regex VideoUrlRegex = new(
        @"^(https?://)?(www\.)?([-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b)([-a-zA-Z0-9()@:%_\+.~#?&//=]*)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );
    private static readonly Regex YoutubePlaylistRegex = new(
        @"^(https?://)?(www\.)?youtube\.com/playlist\?list=[\w-]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );
    
    // List of supported video platforms
    private static readonly HashSet<string> SupportedPlatforms = new()
    {
        "youtube.com", "youtu.be",
        "vimeo.com",
        "dailymotion.com",
        "facebook.com",
        "instagram.com",
        "twitter.com",
        "tiktok.com",
        "twitch.tv",
        "reddit.com",
        "soundcloud.com",
        "bilibili.com"
    };

    public VideoInfoService(YoutubeDL youtubeDL)
    {
        _youtubeDL = youtubeDL;
    }

    public async Task<VideoData> GetVideoInfoAsync(string url)
    {
        try
        {
            var result = await _youtubeDL.RunVideoDataFetch(url);
            if (!result.Success)
            {
                throw new Exception($"Failed to fetch video info: {result.ErrorOutput}");
            }
            return result.Data;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching video info: {ex.Message}", ex);
        }
    }

    public async Task<PlaylistData> GetPlaylistInfoAsync(string url)
    {
        try
        {
            var result = await _youtubeDL.RunPlaylistDataFetch(url);
            if (!result.Success)
            {
                throw new Exception($"Failed to fetch playlist info: {result.ErrorOutput}");
            }
            return result.Data;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching playlist info: {ex.Message}", ex);
        }
    }

    public bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!VideoUrlRegex.IsMatch(url))
        {
            return false;
        }

        try
        {
            var uri = new Uri(url);
            var domain = uri.Host.ToLower();
            domain = domain.StartsWith("www.") ? domain[4..] : domain;
            
            // Check if the domain or any of its parts match supported platforms
            return SupportedPlatforms.Any(platform => domain.EndsWith(platform));
        }
        catch
        {
            return false;
        }
    }

    public bool IsPlaylistUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return YoutubePlaylistRegex.IsMatch(url);
    }

    public async Task<IEnumerable<FormatData>> GetFormatsAsync(string url)
    {
        try
        {
            var result = await _youtubeDL.RunVideoDataFetch(url);
            if (!result.Success)
            {
                throw new Exception($"Failed to fetch video formats: {result.ErrorOutput}");
            }

            return result.Data.Formats ?? Enumerable.Empty<FormatData>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching video formats: {ex.Message}", ex);
        }
    }
}