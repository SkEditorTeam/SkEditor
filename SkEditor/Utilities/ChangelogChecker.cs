using FluentAvalonia.UI.Controls;
using SkEditor.API;

namespace SkEditor.Utilities;
public static class ChangelogChecker
{
    private static string GetVersion() => SkEditorAPI.Core.GetInformationalVersion();

    // This changelog system is pretty lame, will be improved in the future
    private static readonly string[] changelog =
    [
        "Welcome to the new version of SkEditor!",
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
