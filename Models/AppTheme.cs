namespace TreaYT.Models;

public enum AppTheme
{
    System,
    Light,
    Dark
}

public static class AppThemeExtensions
{
    public static string ToDisplayString(this AppTheme theme)
    {
        return theme switch
        {
            AppTheme.System => "System",
            AppTheme.Light => "Light",
            AppTheme.Dark => "Dark",
            _ => "Unknown"
        };
    }

    public static Microsoft.UI.Xaml.ElementTheme ToElementTheme(this AppTheme theme)
    {
        return theme switch
        {
            AppTheme.Light => Microsoft.UI.Xaml.ElementTheme.Light,
            AppTheme.Dark => Microsoft.UI.Xaml.ElementTheme.Dark,
            _ => Microsoft.UI.Xaml.ElementTheme.Default
        };
    }
}