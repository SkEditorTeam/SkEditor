using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Configuration.Abstractions;
using SkEditor.Editor.Syntax.Models;

namespace SkEditor.Configuration.Configs;

[SettingsConfig]
public partial class AppSettings : BaseConfig<AppSettings>
{
    /// <summary>
    ///     The current application theme file name.
    /// </summary>
    [ObservableProperty]
    public partial string CurrentTheme { get; set; } = "Default.json";
    
    [ObservableProperty]
    public partial List<SyntaxPairDto> SyntaxPairs { get; set; } = [];

    /// <summary>
    ///     Whether to use hardware acceleration, if available.
    /// </summary>
    [ObservableProperty]
    public partial bool UseHardwareAcceleration { get; set; } = true;
    
    /// <summary>
    ///     Whether to check for app updates automatically on app startup.
    /// </summary>
    [ObservableProperty]
    public partial bool CheckForUpdates { get; set; } = true;
    
    /// <summary>
    ///     Whether to include pre-releases when checking for updates.
    /// </summary>
    [ObservableProperty]
    public partial bool IncludePreReleaseUpdates { get; set; } = false;

    /// <summary>
    ///     The UI scale factor.
    /// </summary>
    [ObservableProperty]
    public partial double CustomUiScale { get; set; } = 1.0;

    /// <summary>
    ///     The font family used in the code editor. If null, the default font is used.
    /// </summary>
    [ObservableProperty]
    public partial string? EditorFontFamily { get; set; } = null;

    /// <summary>
    ///     The font size used in the code editor.
    /// </summary>
    [ObservableProperty]
    public partial int EditorFontSize { get; set; } = 14;

    /// <summary>
    ///     The number of spaces per tab in the code editor.
    /// </summary>
    [ObservableProperty]
    public partial int EditorTabSize { get; set; } = 4;

    /// <summary>
    ///     If true, SkEditor will use the integrated in-app menu on macOS instead of the native menu bar.
    /// </summary>
    /// <remarks>Does not affect other platforms and shouldn't be visible to users in UI.</remarks>
    [ObservableProperty]
    public partial bool UseIntegratedMenuOnMacOs { get; set; } = false;

    /// <summary>
    ///     Whether to highlight the current line in the editor.
    /// </summary>
    [ObservableProperty]
    public partial bool HighlightCurrentLine { get; set; } = true;

    /// <summary>
    ///     Whether to automatically increment indentation on new lines if the previous line ends with a colon.
    /// </summary>
    [ObservableProperty]
    public partial bool IsAutoIndentEnabled { get; set; }

    /// <summary>
    ///     Whether to automatically insert closing brackets, quotes, etc. when typing opening ones.
    /// </summary>
    [ObservableProperty]
    public partial bool IsAutoPairingEnabled { get; set; }

    /// <summary>
    ///     Automatically saves the user files after every change if enabled.
    /// </summary>
    [ObservableProperty]
    public partial bool IsAutoSaveEnabled { get; set; }

    /// <summary>
    ///     Automatically saves and restores the user files, folders, etc. without any prompts.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSessionRestoreEnabled { get; set; } = false;

    /// <summary>
    ///     Detects external changes to files and prompts the user to reload them if enabled.
    /// </summary>
    [ObservableProperty]
    public partial bool IsChangeDetectEnabled { get; set; } = false;

    /// <summary>
    ///     Whether to enable the developer mode.
    /// </summary>
    [ObservableProperty]
    public partial bool IsDevModeEnabled { get; set; } = false;

    /// <summary>
    ///     Whether to show the current file name in Discord Rich Presence details.
    /// </summary>
    [ObservableProperty]
    public partial bool IsDiscordRichPresenceDetailsEnabled { get; set; } = false;

    /// <summary>
    ///     Whether to use Discord Rich Presence integration.
    /// </summary>
    [ObservableProperty]
    public partial bool IsDiscordRichPresenceEnabled { get; set; } = true;

    /// <summary>
    ///     Whether to enable smooth scrolling in the editor.
    /// </summary>
    [ObservableProperty]
    public partial bool IsEditorSmoothScrollEnabled { get; set; } = true;

    /// <summary>
    ///     Whether to synchronize the zoom level across all open editor tabs.
    /// </summary>
    [ObservableProperty]
    public partial bool IsEditorZoomLevelSyncEnabled { get; set; }

    /// <summary>
    ///     Whether to open files and expand folders in the explorer with a single click instead of a double click
    /// </summary>
    [ObservableProperty]
    public partial bool IsExplorerSingleClickOpenEnabled { get; set; }

    /// <summary>
    ///     Whether to copy file paths with forward slashes instead of backslashes in the explorer.
    /// </summary>
    /// <remarks>Shouldn't be visible to non-Windows users in UI.</remarks>
    [ObservableProperty]
    public partial bool IsExplorerForwardSlashesCopyEnabled { get; set; } = true;

    /// <summary>
    ///     Whether this is (likely) the first time the user runs the application. Used to show the tutorial.
    /// </summary>
    [ObservableProperty]
    public partial bool IsFirstRun { get; set; } = true;

    /// <summary>
    ///     Whether to enable font ligatures in the editor (if supported by the active font).
    /// </summary>
    [ObservableProperty]
    public partial bool IsFontLigaturesEnabled { get; set; } = true;

    /// <summary>
    ///     Whether to enable zooming the editor font size by holding Ctrl and scrolling the mouse wheel.
    /// </summary>
    [ObservableProperty]
    public partial bool IsMouseWheelZoomEnabled { get; set; } = true;

    /// <summary>
    ///     Whether to enable word wrap in the editor.
    /// </summary>
    [ObservableProperty]
    public partial bool IsWordWrapEnabled { get; set; }

    /// <summary>
    ///     The application language code.
    /// </summary>
    [ObservableProperty]
    public partial string Language { get; set; } = "English";

    /// <summary>
    ///     Whether to use tabs for indentation in the code editor. If false, spaces are used.
    /// </summary>
    [ObservableProperty]
    public partial bool UseEditorTabsForIndentation { get; set; }
    
    /// <summary>
    ///     Enables the Color Preview feature.
    /// </summary>
    [ObservableProperty]
    public partial bool ColorPreviewEnabled { get; set; } = false;

    /// <summary>
    ///     Whether the Color Preview feature should be enabled for all the file types or just Skript files
    /// </summary>
    [ObservableProperty]
    public partial bool GlobalColorPreviewEnabled { get; set; } = false;

    /// <summary>
    ///     The application version.
    /// </summary>
    [ObservableProperty]
    public partial string Version { get; set; } = string.Empty;

    protected override JsonTypeInfo<AppSettings> TypeInfo => AppJsonContext.Default.AppSettings;

    protected override string GetFileName()
    {
        return "options.json";
    }
}