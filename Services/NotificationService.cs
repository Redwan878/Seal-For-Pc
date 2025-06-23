using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Windows.Foundation.Collections;

namespace TreaYT.Services;

public class NotificationService : INotificationService
{
    private readonly TaskbarIcon _taskbarIcon;
    private readonly Window _window;

    public NotificationService(TaskbarIcon taskbarIcon, Window window)
    {
        _taskbarIcon = taskbarIcon;
        _window = window;
    }

    public void ShowInformation(string title, string message)
    {
        _taskbarIcon.ShowNotification(
            title,
            message,
            NotificationIcon.Info
        );
    }

    public void ShowSuccess(string title, string message)
    {
        _taskbarIcon.ShowNotification(
            title,
            message,
            NotificationIcon.Success
        );
    }

    public void ShowWarning(string title, string message)
    {
        _taskbarIcon.ShowNotification(
            title,
            message,
            NotificationIcon.Warning
        );
    }

    public void ShowError(string title, string message)
    {
        _taskbarIcon.ShowNotification(
            title,
            message,
            NotificationIcon.Error
        );
    }

    public void ShowDownloadComplete(string title, string outputPath)
    {
        var notification = new Dictionary<string, string>
        {
            { "title", $"Download Complete: {title}" },
            { "message", $"Saved to: {outputPath}" },
            { "outputPath", outputPath }
        };

        _taskbarIcon.ShowNotification(
            notification["title"],
            notification["message"],
            NotificationIcon.Success
        );

        // Bring window to front if minimized
        if (_window.Visible)
        {
            _window.Activate();
        }
    }

    public void ShowDownloadError(string title, string error)
    {
        var notification = new Dictionary<string, string>
        {
            { "title", $"Download Failed: {title}" },
            { "message", $"Error: {error}" }
        };

        _taskbarIcon.ShowNotification(
            notification["title"],
            notification["message"],
            NotificationIcon.Error
        );

        // Bring window to front if minimized
        if (_window.Visible)
        {
            _window.Activate();
        }
    }
}