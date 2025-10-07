using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Utilities;

namespace SkEditor.Views.Windows.Settings;

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
        GitHubItem.Command =
            new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://github.com/SkEditorTeam/SkEditor"));
        DocumentationItem.Command = new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://docs.skeditor.dev"));
        DiscordItem.Command = new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://skeditordc.notro.me/"));
        AppDataItem.Command = new RelayCommand(() => SkEditorAPI.Core.OpenFolder(AppConfig.AppDataFolderPath));
    }
}