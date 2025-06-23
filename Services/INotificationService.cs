namespace TreaYT.Services;

public interface INotificationService
{
    /// <summary>
    /// Shows an information notification.
    /// </summary>
    /// <param name="title">The title of the notification.</param>
    /// <param name="message">The message content of the notification.</param>
    void ShowInformation(string title, string message);

    /// <summary>
    /// Shows a success notification.
    /// </summary>
    /// <param name="title">The title of the notification.</param>
    /// <param name="message">The message content of the notification.</param>
    void ShowSuccess(string title, string message);

    /// <summary>
    /// Shows a warning notification.
    /// </summary>
    /// <param name="title">The title of the notification.</param>
    /// <param name="message">The message content of the notification.</param>
    void ShowWarning(string title, string message);

    /// <summary>
    /// Shows an error notification.
    /// </summary>
    /// <param name="title">The title of the notification.</param>
    /// <param name="message">The message content of the notification.</param>
    void ShowError(string title, string message);

    /// <summary>
    /// Shows a download complete notification.
    /// </summary>
    /// <param name="title">The title of the downloaded video.</param>
    /// <param name="outputPath">The path where the video was saved.</param>
    void ShowDownloadComplete(string title, string outputPath);

    /// <summary>
    /// Shows a download error notification.
    /// </summary>
    /// <param name="title">The title of the video that failed to download.</param>
    /// <param name="error">The error message describing what went wrong.</param>
    void ShowDownloadError(string title, string error);
}