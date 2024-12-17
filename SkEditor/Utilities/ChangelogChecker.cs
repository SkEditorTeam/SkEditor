using FluentAvalonia.UI.Controls;
using SkEditor.API;

namespace SkEditor.Utilities;
public static class ChangelogChecker
{
    private static string GetVersion() => SkEditorAPI.Core.GetInformationalVersion();

    private static readonly string[] changelog =
    [
        "Welcome to the new version of SkEditor!",
        "This update focuses exclusively on bug fixes.",
        "Check the changelog on GitHub to see what's fixed!",
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
