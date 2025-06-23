using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using TreaYT.ViewModels;

namespace TreaYT.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<SettingsViewModel>();
        this.InitializeComponent();
    }
}