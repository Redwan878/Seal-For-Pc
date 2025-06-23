using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace TreaYT.Services;

public class DialogService : IDialogService
{
    private readonly Window _window;

    public DialogService(Window window)
    {
        _window = window;
    }

    public async Task ShowMessageAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = _window.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "No",
            DefaultButton = ContentDialogButton.Secondary,
            XamlRoot = _window.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = _window.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }

    public async Task<string> ShowFolderPickerAsync(string title, string suggestedStartLocation = null)
    {
        var folderPicker = new FolderPicker();
        folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        folderPicker.FileTypeFilter.Add("*");

        // Initialize the folder picker with the window handle
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, WinRT.Interop.WindowNative.GetWindowHandle(_window));

        var folder = await folderPicker.PickSingleFolderAsync();
        return folder?.Path;
    }

    public async Task<string> ShowOpenFilePickerAsync(string title, IEnumerable<string> fileTypes, string suggestedStartLocation = null)
    {
        var filePicker = new FileOpenPicker();
        filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

        foreach (var fileType in fileTypes)
        {
            filePicker.FileTypeFilter.Add(fileType);
        }

        // Initialize the file picker with the window handle
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, WinRT.Interop.WindowNative.GetWindowHandle(_window));

        var file = await filePicker.PickSingleFileAsync();
        return file?.Path;
    }

    public async Task<string> ShowSaveFilePickerAsync(string title, string defaultExtension, string suggestedFileName, IEnumerable<string> fileTypes)
    {
        var savePicker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = suggestedFileName,
            DefaultFileExtension = defaultExtension
        };

        foreach (var fileType in fileTypes)
        {
            savePicker.FileTypeChoices.Add(fileType, new List<string> { fileType });
        }

        // Initialize the save picker with the window handle
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, WinRT.Interop.WindowNative.GetWindowHandle(_window));

        var file = await savePicker.PickSaveFileAsync();
        return file?.Path;
    }

    public async Task<ContentDialogResult> ShowContentDialogAsync(
        string title,
        object content,
        string primaryButtonText,
        string secondaryButtonText = null,
        string closeButtonText = null)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = primaryButtonText,
            SecondaryButtonText = secondaryButtonText,
            CloseButtonText = closeButtonText,
            XamlRoot = _window.Content.XamlRoot,
            DefaultButton = ContentDialogButton.Primary
        };

        return await dialog.ShowAsync();
    }
}