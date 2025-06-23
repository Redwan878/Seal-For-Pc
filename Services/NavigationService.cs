using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace TreaYT.Services;

public class NavigationService : INavigationService
{
    private Frame _frame;
    private readonly IList<Type> _pageHistory;
    private int _currentPageIndex;

    public NavigationService()
    {
        _pageHistory = new List<Type>();
        _currentPageIndex = -1;
    }

    public Type CurrentPage => _frame?.CurrentSourcePageType;

    public bool CanGoBack => _currentPageIndex > 0;

    public bool CanGoForward => _currentPageIndex < _pageHistory.Count - 1;

    public void SetFrame(Frame frame)
    {
        _frame = frame;
        _frame.NavigationFailed += Frame_NavigationFailed;
        _frame.Navigated += Frame_Navigated;
    }

    public bool Navigate(Type pageType)
    {
        return NavigateInternal(pageType, null);
    }

    public bool Navigate(Type pageType, object parameter)
    {
        return NavigateInternal(pageType, parameter);
    }

    private bool NavigateInternal(Type pageType, object parameter)
    {
        if (_frame == null)
        {
            return false;
        }

        // Don't navigate if we're already on the requested page
        if (CurrentPage == pageType)
        {
            return false;
        }

        var navigationResult = _frame.Navigate(
            pageType,
            parameter,
            new DrillInNavigationTransitionInfo()
        );

        if (navigationResult)
        {
            // Remove forward history when navigating to a new page
            if (_currentPageIndex < _pageHistory.Count - 1)
            {
                _pageHistory.RemoveRange(_currentPageIndex + 1, _pageHistory.Count - _currentPageIndex - 1);
            }

            _pageHistory.Add(pageType);
            _currentPageIndex = _pageHistory.Count - 1;
        }

        return navigationResult;
    }

    public bool GoBack()
    {
        if (!CanGoBack || _frame == null)
        {
            return false;
        }

        var navigationResult = _frame.Navigate(
            _pageHistory[_currentPageIndex - 1],
            null,
            new DrillInNavigationTransitionInfo()
        );

        if (navigationResult)
        {
            _currentPageIndex--;
        }

        return navigationResult;
    }

    public bool GoForward()
    {
        if (!CanGoForward || _frame == null)
        {
            return false;
        }

        var navigationResult = _frame.Navigate(
            _pageHistory[_currentPageIndex + 1],
            null,
            new DrillInNavigationTransitionInfo()
        );

        if (navigationResult)
        {
            _currentPageIndex++;
        }

        return navigationResult;
    }

    public void ClearHistory()
    {
        _pageHistory.Clear();
        _currentPageIndex = -1;
        _frame?.BackStack.Clear();
        _frame?.ForwardStack.Clear();
    }

    private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        // Log navigation failures
        System.Diagnostics.Debug.WriteLine($"Navigation failed: {e.Exception.Message}");
    }

    private void Frame_Navigated(object sender, NavigationEventArgs e)
    {
        // Handle post-navigation tasks if needed
        System.Diagnostics.Debug.WriteLine($"Navigated to: {e.SourcePageType.Name}");
    }
}