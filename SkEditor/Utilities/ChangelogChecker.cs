using FluentAvalonia.UI.Controls;
using SkEditor.API;

namespace SkEditor.Utilities;
public static class ChangelogChecker
{
    private static string GetVersion() => SkEditorAPI.Core.GetInformationalVersion();

    private static readonly string[] changelog =
    [
        "Welcome to the new version of SkEditor!",
        "This version is PRE-RELEASE! If you find any bugs, please report them to the SkEditor Discord server.",
        "",
        "🔧 The addon system was completely overhauled. For more information, check the changelog on the GitHub repository.",
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
