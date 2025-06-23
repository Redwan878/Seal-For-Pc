using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace TreaYT.Services;

public class UpdateService : IUpdateService
{
    private readonly HttpClient _httpClient;
    private readonly string _githubApiUrl;
    private readonly string _owner;
    private readonly string _repo;
    private bool _isUpdateAvailable;
    private string _latestVersion;
    private bool _isUpdating;
    private string _latestReleaseNotes;
    private string _downloadUrl;

    public event EventHandler<string> UpdateAvailable;
    public event EventHandler<double> UpdateProgressChanged;
    public event EventHandler<bool> UpdateCompleted;

    public bool IsUpdateAvailable => _isUpdateAvailable;
    public string LatestVersion => _latestVersion;
    public string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    public bool IsUpdating => _isUpdating;

    public UpdateService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TreaYT-App");
        _owner = "firozalambda"; // Replace with your GitHub username
        _repo = "treayt"; // Replace with your repository name
        _githubApiUrl = $"https://api.github.com/repos/{_owner}/{_repo}/releases/latest";
    }

    public async Task<bool> CheckForUpdatesAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(_githubApiUrl);
            var release = JsonSerializer.Deserialize<GitHubRelease>(response);

            if (release != null)
            {
                _latestVersion = release.TagName.TrimStart('v');
                _latestReleaseNotes = release.Body;
                _downloadUrl = release.Assets.FirstOrDefault()?.BrowserDownloadUrl;

                var current = Version.Parse(CurrentVersion);
                var latest = Version.Parse(_latestVersion);

                _isUpdateAvailable = latest > current;

                if (_isUpdateAvailable)
                {
                    UpdateAvailable?.Invoke(this, _latestVersion);
                }

                return _isUpdateAvailable;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking for updates: {ex.Message}");
        }

        return false;
    }

    public async Task<bool> UpdateAsync()
    {
        if (!_isUpdateAvailable || string.IsNullOrEmpty(_downloadUrl))
        {
            return false;
        }

        try
        {
            _isUpdating = true;

            // Download the update
            var tempFile = Path.Combine(Path.GetTempPath(), "TreaYT_Update.msix");
            using (var response = await _httpClient.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var readBytes = 0L;

                using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var downloadStream = await response.Content.ReadAsStreamAsync())
                {
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    do
                    {
                        var read = await downloadStream.ReadAsync(buffer);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await fileStream.WriteAsync(buffer.AsMemory(0, read));
                            readBytes += read;

                            if (totalBytes != -1)
                            {
                                var progress = (double)readBytes / totalBytes;
                                UpdateProgressChanged?.Invoke(this, progress);
                            }
                        }
                    }
                    while (isMoreToRead);
                }
            }

            // Launch the installer
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = tempFile,
                UseShellExecute = true
            };

            System.Diagnostics.Process.Start(startInfo);

            _isUpdating = false;
            UpdateCompleted?.Invoke(this, true);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating application: {ex.Message}");
            _isUpdating = false;
            UpdateCompleted?.Invoke(this, false);
            return false;
        }
    }

    public Task<string> GetReleaseNotesAsync()
    {
        return Task.FromResult(_latestReleaseNotes ?? string.Empty);
    }

    public Task<string> GetDownloadUrlAsync()
    {
        return Task.FromResult(_downloadUrl ?? string.Empty);
    }

    private class GitHubRelease
    {
        public string TagName { get; set; }
        public string Body { get; set; }
        public List<GitHubAsset> Assets { get; set; }
    }

    private class GitHubAsset
    {
        public string BrowserDownloadUrl { get; set; }
    }
}