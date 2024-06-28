using FluentAvalonia.UI.Controls;
using SkEditor.API;
using System.Reflection;

namespace SkEditor.Utilities;
public static class ChangelogChecker
{
    private static string GetVersion() => $"{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Build}";

    private static readonly string[] changelog =
    [
        "Welcome to the new version of SkEditor!",
        "This version includes the following changes:",
        "🔬 Added Session Restoring: SkEditor now remembers your last opened files! Enable as experiment in the settings.",
        "🗑️ Added more closing options to the menu.",
        "🅰️ The default font now supports bold and italic styles.",
    ];

    public async static void Check()
    {
        string version = SkEditorAPI.Core.GetAppConfig().Version;
        if (version == GetVersion()) return;

        await SkEditorAPI.Windows.ShowDialog($"v{GetVersion()} 🚀", string.Join('\n', changelog), icon: null);

        SkEditorAPI.Core.GetAppConfig().Version = GetVersion();
    }
}
