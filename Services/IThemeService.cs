using Microsoft.UI.Xaml;

namespace TreaYT.Services;

public enum AppTheme
{
    System,
    Light,
    Dark
}

public interface IThemeService
{
    void Initialize(Window window);
    void ApplyTheme(AppTheme theme);
    AppTheme GetCurrentTheme();
    bool IsDarkTheme();
}

public class ThemeService : IThemeService
{
    private Window? _window;
    private AppTheme _currentTheme = AppTheme.System;

    public void Initialize(Window window)
    {
        _window = window;
    }

    public void ApplyTheme(AppTheme theme)
    {
        if (_window == null) return;

        _currentTheme = theme;
        var rootElement = _window.Content as FrameworkElement;
        if (rootElement == null) return;

        switch (theme)
        {
            case AppTheme.Light:
                rootElement.RequestedTheme = ElementTheme.Light;
                break;
            case AppTheme.Dark:
                rootElement.RequestedTheme = ElementTheme.Dark;
                break;
            default: // System
                rootElement.RequestedTheme = ElementTheme.Default;
                break;
        }

        // Update resource dictionaries
        UpdateResourcesForTheme(IsDarkTheme());
    }

    public AppTheme GetCurrentTheme() => _currentTheme;

    public bool IsDarkTheme()
    {
        if (_window == null) return false;

        var rootElement = _window.Content as FrameworkElement;
        if (rootElement == null) return false;

        if (_currentTheme == AppTheme.System)
        {
            // Get system theme
            var systemTheme = Application.Current.RequestedTheme;
            return systemTheme == ApplicationTheme.Dark;
        }

        return _currentTheme == AppTheme.Dark;
    }

    private void UpdateResourcesForTheme(bool isDark)
    {
        var resources = Application.Current.Resources;
        
        // Update brushes based on theme
        if (isDark)
        {
            resources["PrimaryBrush"].GetType().GetProperty("Color")?.SetValue(
                resources["PrimaryBrush"], resources["PrimaryColorDark"]);
            // Add similar updates for other brushes
        }
        else
        {
            resources["PrimaryBrush"].GetType().GetProperty("Color")?.SetValue(
                resources["PrimaryBrush"], resources["PrimaryColor"]);
            // Add similar updates for other brushes
        }
    }
}