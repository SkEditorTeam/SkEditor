using SkEditor.API;
using SkEditor.Utilities;

namespace SkEditor.ViewModels;

public class SettingsViewModel
{
    public static bool IsDiscordRpcEnabled { get; set; } = SkEditorAPI.Core.GetAppConfig().IsDiscordRpcEnabled;
    public static bool IsWrappingEnabled { get; set; } = SkEditorAPI.Core.GetAppConfig().IsWrappingEnabled;
    public static bool IsAutoIndentEnabled { get; set; } = SkEditorAPI.Core.GetAppConfig().IsAutoIndentEnabled;
    public static bool IsAutoPairingEnabled { get; set; } = SkEditorAPI.Core.GetAppConfig().IsAutoPairingEnabled;
    public static bool IsAutoSaveEnabled { get; set; } = SkEditorAPI.Core.GetAppConfig().IsAutoSaveEnabled;
    public static bool CheckForUpdates { get; set; } = SkEditorAPI.Core.GetAppConfig().CheckForUpdates;
    public static bool CheckForChanges { get; set; } = SkEditorAPI.Core.GetAppConfig().CheckForChanges;

    public static bool IsPasteIndentationEnabled { get; set; } =
        SkEditorAPI.Core.GetAppConfig().IsPasteIndentationEnabled;

    public static bool UseSkriptGui { get; set; } = SkEditorAPI.Core.GetAppConfig().UseSkriptGui;

    public static string Version { get; set; } = Translation.Get("SettingsAboutVersionDescription").Replace("{0}",
        $"{UpdateChecker.Major}.{UpdateChecker.Minor}.{UpdateChecker.Build}");

    public static string CurrentFont { get; set; } = Translation.Get("SettingsPersonalizationFontDescription")
        .Replace("{0}", SkEditorAPI.Core.GetAppConfig().Font);
}