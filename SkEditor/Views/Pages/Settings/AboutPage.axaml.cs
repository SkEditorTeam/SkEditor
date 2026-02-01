using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;

namespace SkEditor.Views.Pages.Settings;

public partial class AboutPage : UserControl
{
    public AboutPage()
    {
        InitializeComponent();
        AssignCommands();
    }

    private void AssignCommands()
    {
        GitHubItem.Command =
            new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://github.com/SkEditorTeam/SkEditor"));
        SupportSkEditorItem.Command = new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://ko-fi.com/notro"));
        DocumentationItem.Command = new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://docs.skeditor.dev"));
        DiscordItem.Command = new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://discord.skeditor.dev"));
        AppDataItem.Command = new RelayCommand(() => SkEditorAPI.Core.OpenFolder(SkEditorAPI.Configs.GetAppDataPath()));
    }
}