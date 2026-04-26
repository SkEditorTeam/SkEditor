using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Settings.Attributes;

namespace SkEditor.Utilities.Settings.Configs;

[SettingsConfig]
public partial class AppSettings : BaseConfig<AppSettings>
{
    protected override JsonTypeInfo<AppSettings> TypeInfo => AppJsonContext.Default.AppSettings;

    /// <summary>
    ///     The application version.
    /// </summary>
    [ObservableProperty]
    public partial string Version { get; set; } = string.Empty;

    /// <summary>
    ///    Whether this is (likely) the first time the user runs the application. Used to show the tutorial.
    /// </summary>
    [ObservableProperty]
    public partial string IsFirstRun { get; set; } = string.Empty;

    /// <summary>
    ///     Whether to enable the developer mode.
    /// </summary>
    [ObservableProperty]
    public partial bool IsDevModeEnabled { get; set; } = false;

    /// <summary>
    ///     The application language code.
    /// </summary>
    [ObservableProperty]
    public partial string Language { get; set; } = "English";

    /// <summary>
    ///     The current application theme file name.
    /// </summary>
    [ObservableProperty]
    public partial string CurrentTheme { get; set; } = "Default.json";

    /// <summary>
    ///     A dictionary mapping file extensions to their associated syntax highlighting themes.
    /// </summary>
    [ObservableProperty]
    public partial Dictionary<string, string> FileSyntaxes { get; set; } = new();

    /// <summary>
    ///     Whether to check for application updates on startup.
    /// </summary>
    [ObservableProperty]
    public partial bool CheckForAppUpdates { get; set; } = true;

    /// <summary>
    ///     Whether to enable automatic checks if the file was modified by an external program.
    /// </summary>
    [ObservableProperty]
    public partial bool CheckForExternalFileChanges { get; set; } = true;

    /// <summary>
    ///     The UI scale factor.
    /// </summary>
    [ObservableProperty]
    public partial double CustomUiScale { get; set; } = 1.0;

    /// <summary>
    ///     Whether to force the native window title bar on Windows.
    /// </summary>
    [ObservableProperty]
    public partial bool ForceNativeTitleBar { get; set; }

    /// <summary>
    ///     Whether to use Discord Rich Presence integration.
    /// </summary>
    [ObservableProperty]
    public partial bool IsDiscordRichPresenceEnabled { get; set; } = true;

    /// <summary>
    ///     Whether to enable single-click to open project folders in the Project Sidebar.
    /// </summary>
    [ObservableProperty]
    public partial bool IsProjectSingleClickEnabled { get; set; }

    /// <summary>
    ///     Whether to enable sidebar animations.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSidebarAnimationEnabled { get; set; }

    /// <summary>
    ///     Whether to synchronize the width of all panels in the sidebar.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSidebarWidthSyncEnabled { get; set; }

    /// <summary>
    ///     Automatically saves the user files after every change if enabled.
    /// </summary>
    [ObservableProperty]
    public partial bool IsAutoSaveEnabled { get; set; }

    /// <summary>
    ///     Whether to automatically insert closing brackets, quotes, etc. when typing opening ones.
    /// </summary>
    [ObservableProperty]
    public partial bool IsAutoPairingEnabled { get; set; }

    /// <summary>
    ///     Whether to automatically increment indentation on new lines if the previous line ends with a colon.
    /// </summary>
    [ObservableProperty]
    public partial bool IsAutoIndentEnabled { get; set; }

    /// <summary>
    ///     Whether to make editor try to adapt pasted code indentation to the current line indentation.
    /// </summary>
    [ObservableProperty]
    public partial bool IsPasteIndentationEnabled { get; set; }

    /// <summary>
    ///     Whether to highlight the current line in the editor.
    /// </summary>
    [ObservableProperty]
    public partial bool HighlightCurrentLine { get; set; } = true;

    /// <summary>
    ///     Whether to enable word wrap in the editor.
    /// </summary>
    [ObservableProperty]
    public partial bool IsWordWrapEnabled { get; set; }

    /// <summary>
    ///     Whether to synchronize the zoom level across all open editor tabs.
    /// </summary>
    [ObservableProperty]
    public partial bool IsEditorZoomLevelSyncEnabled { get; set; }

    /// <summary>
    ///     Whether to enable smooth scrolling in the editor.
    /// </summary>
    [ObservableProperty]
    public partial bool IsEditorSmoothScrollEnabled { get; set; } = true;

    /// <summary>
    ///     The font size used in the code editor.
    /// </summary>
    [ObservableProperty]
    public partial int EditorFontSize { get; set; } = 16;

    /// <summary>
    ///     The font family used in the code editor. If null, the default font is used.
    /// </summary>
    [ObservableProperty]
    public partial string? EditorFontFamily { get; set; } = null;

    /// <summary>
    ///     The number of spaces per tab in the code editor.
    /// </summary>
    [ObservableProperty]
    public partial int EditorTabSize { get; set; } = 4;

    /// <summary>
    ///     Whether to use tabs for indentation in the code editor. If false, spaces are used.
    /// </summary>
    [ObservableProperty]
    public partial bool UseEditorTabsForIndentation { get; set; }

    protected override string GetFileName()
    {
        return "options.json";
    }
}