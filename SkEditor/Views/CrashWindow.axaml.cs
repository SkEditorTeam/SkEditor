using FluentAvalonia.UI.Windowing;
using SkEditor.API;

namespace SkEditor.Views;

public partial class CrashWindow : AppWindow
{
    public CrashWindow(string exception)
    {
        InitializeComponent();

        CrashStackTrace.Text = exception;
        AssignCommands();
    }

    public void AssignCommands()
    {
        DiscordButton.Click += (_, _) => OpenDiscord();
        CloseButton.Click += (_, _) => CloseWindow();
    }

    public void OpenDiscord()
    {
        SkEditorAPI.Core.OpenLink("https://skeditordc.notro.me/");
    }

    public void CloseWindow()
    {
        Close();
    }
}