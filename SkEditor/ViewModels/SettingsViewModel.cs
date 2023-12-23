using SkEditor.API;
using SkEditor.Utilities;
using System.Reflection;

namespace SkEditor.ViewModels;
public class SettingsViewModel
{
    public static bool IsDiscordRpcEnabled { get; set; } = ApiVault.Get().GetAppConfig().IsDiscordRpcEnabled;
    public static bool IsWrappingEnabled { get; set; } = ApiVault.Get().GetAppConfig().IsWrappingEnabled;
    public static bool IsAutoIndentEnabled { get; set; } = ApiVault.Get().GetAppConfig().IsAutoIndentEnabled;
    public static bool IsAutoPairingEnabled { get; set; } = ApiVault.Get().GetAppConfig().IsAutoPairingEnabled;
    public static bool IsAutoSaveEnabled { get; set; } = ApiVault.Get().GetAppConfig().IsAutoSaveEnabled;
    public static bool CheckForUpdates { get; set; } = ApiVault.Get().GetAppConfig().CheckForUpdates;
    public static bool CheckForChanges { get; set; } = ApiVault.Get().GetAppConfig().CheckForChanges;

    public static bool UseSkriptGui { get; set; } = ApiVault.Get().GetAppConfig().UseSkriptGui;

    public static string Version { get; set; } = Translation.Get("SettingsAboutVersionDescription").Replace("{0}", $"{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Build}");

    public static string CurrentFont { get; set; } = Translation.Get("SettingsPersonalizationFontDescription").Replace("{0}", ApiVault.Get().GetAppConfig().Font);
}
