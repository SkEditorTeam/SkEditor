using FluentAvalonia.UI.Controls;
using SkEditor.API;

namespace SkEditor.Utilities;
public static class ChangelogChecker
{
    private static string GetVersion() => SkEditorAPI.Core.GetInformationalVersion();

    private static readonly string[] changelog =
    [
        "Welcome to the new version of SkEditor!",
        "The last two updates were pre-releases, so you might have missed some important changes.",
        "Check the changelog on GitHub to see what's new!"
    ];

    public async static void Check()
    {
        string version = SkEditorAPI.Core.GetAppConfig().Version;
        if (version == GetVersion()) return;

        FontIconSource rocketIcon = new() { Glyph = "🚀" };

        await SkEditorAPI.Windows.ShowDialog($"v{GetVersion()}", string.Join('\n', changelog), icon: rocketIcon);

        SkEditorAPI.Core.GetAppConfig().Version = GetVersion();
    }
}
