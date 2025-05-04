using FluentAvalonia.UI.Controls;
using SkEditor.API;

namespace SkEditor.Utilities;

public static class ChangelogChecker
{
    // This changelog system is pretty lame, will be improved in the future
    private static readonly string[] changelog =
    [
        "Welcome to the new version of SkEditor!"
    ];

    private static string GetVersion()
    {
        return SkEditorAPI.Core.GetInformationalVersion();
    }

    public static async void Check()
    {
        string version = SkEditorAPI.Core.GetAppConfig().Version;
        if (version == GetVersion())
        {
            return;
        }

        FontIconSource rocketIcon = new() { Glyph = "🚀" };

        await SkEditorAPI.Windows.ShowDialog($"v{GetVersion()}", string.Join('\n', changelog), rocketIcon);

        SkEditorAPI.Core.GetAppConfig().Version = GetVersion();
    }
}