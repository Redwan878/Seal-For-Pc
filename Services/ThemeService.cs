using Microsoft.UI.Xaml;
using TreaYT.Models;

namespace TreaYT.Services;

public class ThemeService : IThemeService
{
    private readonly ISettingsService _settingsService;
    private AppTheme _currentTheme;

    public event EventHandler<AppTheme> ThemeChanged;

    public AppTheme CurrentTheme => _currentTheme;

    public ElementTheme CurrentElementTheme => AppThemeToElementTheme(_currentTheme);

    public ThemeService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _currentTheme = _settingsService.GetTheme();
    }

    public void SetTheme(AppTheme theme)
    {
        if (_currentTheme != theme)
        {
            _currentTheme = theme;
            _settingsService.SetTheme(theme);
            ThemeChanged?.Invoke(this, theme);
        }
    }

    public void SetThemeForWindow(Window window, AppTheme theme)
    {
        if (window?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = AppThemeToElementTheme(theme);
        }
    }

    public ElementTheme GetSystemTheme()
    {
        // Get the system theme from Windows settings
        // For now, we'll default to Light as getting the actual system theme
        // requires additional Windows Runtime APIs
        return ElementTheme.Default;
    }

    public ElementTheme AppThemeToElementTheme(AppTheme theme)
    {
        return theme switch
        {
            AppTheme.Light => ElementTheme.Light,
            AppTheme.Dark => ElementTheme.Dark,
            AppTheme.System => ElementTheme.Default,
            _ => ElementTheme.Default
        };
    }
}