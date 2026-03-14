using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.ViewModels.Pages;
using SkEditor.Views.Windows.Settings;

namespace SkEditor.Views.Pages.Settings.Personalization;

public partial class ThemePage : UserControl
{
    public ThemePage()
    {
        InitializeComponent();
        AssignCommands();
    }

    public ThemePage(ThemePageViewModel themePageViewModel) : this() => DataContext = themePageViewModel;

    private void AssignCommands()
    {
        EditThemeItem.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(EditThemePage)));
    }
}