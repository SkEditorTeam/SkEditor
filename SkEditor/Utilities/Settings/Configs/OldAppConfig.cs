using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkEditor.Utilities.Settings.Configs;

/// <summary>
///    Represents the old application configuration structure for migration purposes.
/// </summary>
public class OldAppConfig
{
    public required bool CheckForChanges { get; set; }
    public required bool CheckForUpdates { get; set; }
    public required string CurrentTheme { get; set; }
    public required double CustomUiScale { get; set; }
    public required Dictionary<string, string> FileSyntaxes { get; set; }
    public required string Font { get; set; }
    public required bool ForceNativeTitleBar { get; set; }
    public required bool HighlightCurrentLine { get; set; }
    public required bool IsAutoIndentEnabled { get; set; }
    public required bool IsAutoPairingEnabled { get; set; }
    public required bool IsAutoSaveEnabled { get; set; }
    public required bool IsDevModeEnabled { get; set; }
    public required bool IsDiscordRpcEnabled { get; set; }
    public required bool IsPasteIndentationEnabled { get; set; }
    public required bool IsProjectSingleClickEnabled { get; set; }
    public required bool IsSidebarAnimationEnabled { get; set; }
    public required bool IsWrappingEnabled { get; set; }
    public required bool IsZoomSyncEnabled { get; set; }
    public required bool IsSidebarWidthSyncEnabled { get; set; }
    public required string Language { get; set; }
    public required string LastUsedPublishService { get; set; }
    
    public required int TabSize { get; set; }
    public required bool UseSkriptGui { get; set; }
    public required bool UseSpacesInsteadOfTabs { get; set; }
    public required string Version { get; set; }
    public required Dictionary<string, int> SidebarPanelSizes { get; set; }
    public required Dictionary<string, string?> FileTypeChoices { get; set; }
    public required bool EnableAutoCompletionExperiment { get; set; }
    public required bool EnableProjectsExperiment { get; set; }
    public required bool EnableHexPreview { get; set; }
    public required bool EnableCodeParser { get; set; }
    public required bool EnableFolding { get; set; }
    public required bool EnableBetterPairing { get; set; }
    public required bool EnableSessionRestoring { get; set; }
    public required bool EnableRealtimeCodeParser { get; set; }

    public required string SkUnityApiKey { get; set; }
    public required string SkriptMcApiKey { get; set; }
    public required string PastebinApiKey { get; set; }
    public required string CodeSkriptPlApiKey { get; set; }
}

[JsonSerializable(typeof(OldAppConfig))]
public partial class OldAppJsonContext : JsonSerializerContext;