using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.ViewModels;

namespace SkEditor.Views.Settings;
public partial class AboutPage : UserControl
{
    public AboutPage()
    {
        InitializeComponent();

        AssignCommands();

        DataContext = SkEditorAPI.Core.GetAppConfig();
    }

    private void AssignCommands()
    {
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));

        GitHubItem.Command = new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://github.com/SkEditorTeam/SkEditor"));
        DiscordItem.Command = new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://skeditordc.notro.me/"));
        AppDataItem.Command = new RelayCommand(() => SkEditorAPI.Core.OpenFolder(AppConfig.AppDataFolderPath));
    }
}
