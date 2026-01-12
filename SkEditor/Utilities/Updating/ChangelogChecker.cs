using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using SkEditor.API;

namespace SkEditor.Utilities.Updating;

public static class ChangelogChecker
{
    // This changelog system is pretty lame, will be improved in the future
    private static readonly string[] Changelog =
    [
        "Welcome to the new version of SkEditor!"
    ];

    private static string GetVersion()
    {
        return SkEditorAPI.Core.GetInformationalVersion();
    }

    public static async Task Check()
    {
        string version = SkEditorAPI.Configs.GetAppSettings().Version;

        if (version == GetVersion())
        {
            return;
        }

        FontIconSource rocketIcon = new() { Glyph = "🚀" };

        await SkEditorAPI.Windows.ShowDialog($"v{GetVersion()}", string.Join('\n', Changelog), rocketIcon);

        SkEditorAPI.Configs.GetAppSettings().Version = GetVersion();
        await SkEditorAPI.Configs.GetAppSettings().Save();
    }
}