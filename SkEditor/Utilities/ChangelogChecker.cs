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
        string version = ApiVault.Get().GetAppConfig().Version;
        if (version == GetVersion()) return;

        await ApiVault.Get().ShowAdvancedMessage($"v{GetVersion()} 🚀", string.Join('\n', changelog), primaryButton: false, closeButtonContent: "OK");

        ApiVault.Get().GetAppConfig().Version = GetVersion();
    }
}
