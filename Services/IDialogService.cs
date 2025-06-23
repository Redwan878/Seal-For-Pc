using Microsoft.UI.Xaml;

namespace TreaYT.Services;

public interface IDialogService
{
    /// <summary>
    /// Shows a message dialog with the specified content.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message content of the dialog.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ShowMessageAsync(string title, string message);

    /// <summary>
    /// Shows a confirmation dialog with Yes/No buttons.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message content of the dialog.</param>
    /// <returns>True if the user clicked Yes, false otherwise.</returns>
    Task<bool> ShowConfirmationAsync(string title, string message);

    /// <summary>
    /// Shows an error dialog with the specified content.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The error message content of the dialog.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ShowErrorAsync(string title, string message);

    /// <summary>
    /// Shows a folder picker dialog.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="suggestedStartLocation">The suggested start location for the folder picker.</param>
    /// <returns>The selected folder path, or null if the user cancelled.</returns>
    Task<string> ShowFolderPickerAsync(string title, string suggestedStartLocation = null);

    /// <summary>
    /// Shows a file picker dialog for opening files.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="fileTypes">The allowed file types (e.g., ".exe", ".txt").</param>
    /// <param name="suggestedStartLocation">The suggested start location for the file picker.</param>
    /// <returns>The selected file path, or null if the user cancelled.</returns>
    Task<string> ShowOpenFilePickerAsync(string title, IEnumerable<string> fileTypes, string suggestedStartLocation = null);

    /// <summary>
    /// Shows a file picker dialog for saving files.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="defaultExtension">The default file extension.</param>
    /// <param name="suggestedFileName">The suggested file name.</param>
    /// <param name="fileTypes">The allowed file types (e.g., ".mp4", ".mp3").</param>
    /// <returns>The selected file path, or null if the user cancelled.</returns>
    Task<string> ShowSaveFilePickerAsync(string title, string defaultExtension, string suggestedFileName, IEnumerable<string> fileTypes);

    /// <summary>
    /// Shows a custom content dialog.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="content">The content of the dialog.</param>
    /// <param name="primaryButtonText">The text for the primary button.</param>
    /// <param name="secondaryButtonText">The text for the secondary button (optional).</param>
    /// <param name="closeButtonText">The text for the close button (optional).</param>
    /// <returns>The dialog result indicating which button was clicked.</returns>
    Task<ContentDialogResult> ShowContentDialogAsync(
        string title,
        object content,
        string primaryButtonText,
        string secondaryButtonText = null,
        string closeButtonText = null
    );
}