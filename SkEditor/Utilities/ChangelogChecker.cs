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
        "✨ Added a Code Parser to the sidebar (you need to set EnableProjectsExperiment to true in the settings.json file).",
        "🔨 Significantly improved folder explorer.",
        "🔧 Added an indentation configuration in the settings.",
        "🖼️ Added image support.",
        "🎨 Added the ability to enable the Mica effect in the theme.",
        "🐛 Fixed various bugs and crashes."
    ];

    public async static void Check()
    {
        string version = ApiVault.Get().GetAppConfig().Version;
        if (version == GetVersion()) return;

        await ApiVault.Get().ShowAdvancedMessage($"v{GetVersion()} 🚀", string.Join('\n', changelog), primaryButton: false, closeButtonContent: "OK");

        ApiVault.Get().GetAppConfig().Version = GetVersion();
    }
}
