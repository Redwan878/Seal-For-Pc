namespace TreaYT.Services;

public interface IUpdateService
{
    /// <summary>
    /// Gets whether an update is available.
    /// </summary>
    bool IsUpdateAvailable { get; }

    /// <summary>
    /// Gets the latest version available.
    /// </summary>
    string LatestVersion { get; }

    /// <summary>
    /// Gets the current installed version.
    /// </summary>
    string CurrentVersion { get; }

    /// <summary>
    /// Gets whether an update is in progress.
    /// </summary>
    bool IsUpdating { get; }

    /// <summary>
    /// Checks for available updates.
    /// </summary>
    /// <returns>True if an update is available, false otherwise.</returns>
    Task<bool> CheckForUpdatesAsync();

    /// <summary>
    /// Downloads and installs the latest update.
    /// </summary>
    /// <returns>True if the update was successful, false otherwise.</returns>
    Task<bool> UpdateAsync();

    /// <summary>
    /// Gets the release notes for the latest version.
    /// </summary>
    /// <returns>The release notes as a string.</returns>
    Task<string> GetReleaseNotesAsync();

    /// <summary>
    /// Gets the download URL for the latest version.
    /// </summary>
    /// <returns>The download URL as a string.</returns>
    Task<string> GetDownloadUrlAsync();

    /// <summary>
    /// Event that is raised when an update is available.
    /// </summary>
    event EventHandler<string> UpdateAvailable;

    /// <summary>
    /// Event that is raised when the update progress changes.
    /// </summary>
    event EventHandler<double> UpdateProgressChanged;

    /// <summary>
    /// Event that is raised when the update is completed.
    /// </summary>
    event EventHandler<bool> UpdateCompleted;
}