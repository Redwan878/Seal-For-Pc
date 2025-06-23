using Microsoft.UI.Xaml.Controls;

namespace TreaYT.Services;

public interface INavigationService
{
    /// <summary>
    /// Gets the current page type.
    /// </summary>
    Type CurrentPage { get; }

    /// <summary>
    /// Gets whether navigation can go back.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Gets whether navigation can go forward.
    /// </summary>
    bool CanGoForward { get; }

    /// <summary>
    /// Navigates to the specified page type.
    /// </summary>
    /// <param name="pageType">The type of the page to navigate to.</param>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    bool Navigate(Type pageType);

    /// <summary>
    /// Navigates to the specified page type with parameters.
    /// </summary>
    /// <param name="pageType">The type of the page to navigate to.</param>
    /// <param name="parameter">The parameter to pass to the page.</param>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    bool Navigate(Type pageType, object parameter);

    /// <summary>
    /// Navigates back to the previous page.
    /// </summary>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    bool GoBack();

    /// <summary>
    /// Navigates forward to the next page.
    /// </summary>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    bool GoForward();

    /// <summary>
    /// Sets the navigation frame for the service.
    /// </summary>
    /// <param name="frame">The navigation frame to use.</param>
    void SetFrame(Frame frame);

    /// <summary>
    /// Clears the navigation history.
    /// </summary>
    void ClearHistory();
}