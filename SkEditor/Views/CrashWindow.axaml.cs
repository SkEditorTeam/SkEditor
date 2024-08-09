using Avalonia.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;

namespace SkEditor.Views;

public partial class CrashWindow : AppWindow
{
    public CrashWindow(string exception)
    {
        InitializeComponent();
        Focusable = true;

        CrashStackTrace.Text = exception;
        AssignCommands();
    }

    public void AssignCommands()
    {
        DiscordButton.Click += (_, _) => OpenDiscord();
        CloseButton.Click += (_, _) => Close();
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) Close();
        };
    }

    public static void OpenDiscord() => SkEditorAPI.Core.OpenLink("https://skeditordc.notro.me/");
}