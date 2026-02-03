using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Attributes;
using SkEditor.Utilities.Settings.Providers;

namespace SkEditor.Utilities.Settings.Configs;

[SettingsConfig]
public partial class AppSettings : BaseConfig<AppSettings>
{
    protected override JsonTypeInfo<AppSettings> TypeInfo => AppJsonContext.Default.AppSettings;

    [JsonIgnore]
    public override string FileName => "options.json";

    /// <summary>
    ///     The application version.
    /// </summary>
    public string Version
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>
    ///     Whether to enable the developer mode.
    /// </summary>
    [ToggleSetting("SettingsAboutDevMode", Symbol.Flash, "SettingsGeneralTitle", "SettingsAboutDevModeDescription")]
    public bool IsDevModeEnabled
    {
        get;
        set => SetProperty(ref field, value);
    } = false;

    /// <summary>
    ///     The application language code.
    /// </summary>
    [ChoiceSetting("SettingsGeneralLanguage", Symbol.Globe, "SettingsGeneralTitle", typeof(LanguageSettingsProvider))]
    public string Language
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                _ = Translation.ChangeAndRefreshLanguage(value);
            }
        }
    } = "English";

    /// <summary>
    ///     The current application theme file name.
    /// </summary>
    public string CurrentTheme
    {
        get;
        set => SetProperty(ref field, value);
    } = "Default.json";

    /// <summary>
    ///     A dictionary mapping file extensions to their associated syntax highlighting themes.
    /// </summary>
    public Dictionary<string, string> FileSyntaxes
    {
        get;
        set => SetProperty(ref field, value);
    } = new();

    /// <summary>
    ///     Whether to check for application updates on startup.
    /// </summary>
    [ToggleSetting("SettingsGeneralCheckForUpdates", Symbol.CloudSync, "SettingsGeneralTitle", "SettingsGeneralCheckForUpdatesDescription")]
    public bool CheckForAppUpdates
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <summary>
    ///     Whether to enable automatic checks if the file was modified by an external program.
    /// </summary>
    public bool CheckForExternalFileChanges
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <summary>
    ///     The UI scale factor.
    /// </summary>
    public double CustomUiScale
    {
        get;
        set => SetProperty(ref field, value);
    } = 1.0;

    /// <summary>
    ///     Whether to force the native window title bar on Windows.
    /// </summary>
    public bool ForceNativeTitleBar
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    ///     Whether to use Discord Rich Presence integration.
    /// </summary>
    public bool IsDiscordRichPresenceEnabled
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <summary>
    ///     Whether to enable single-click to open project folders in the Project Sidebar.
    /// </summary>
    public bool IsProjectSingleClickEnabled
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    ///     Whether to enable sidebar animations.
    /// </summary>
    public bool IsSidebarAnimationEnabled
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    ///     Whether to synchronize the width of all panels in the sidebar.
    /// </summary>
    public bool IsSidebarWidthSyncEnabled
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    ///     Automatically saves the user files after every change if enabled.
    /// </summary>
    public bool IsAutoSaveEnabled
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    ///     Whether to automatically insert closing brackets, quotes, etc. when typing opening ones.
    /// </summary>
    public bool IsAutoPairingEnabled
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    ///     Whether to automatically increment indentation on new lines if the previous line ends with a colon.
    /// </summary>
    public bool IsAutoIndentEnabled
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    ///     Whether to make editor try to adapt pasted code indentation to the current line indentation.
    /// </summary>
    public bool IsPasteIndentationEnabled
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    ///     Whether to highlight the current line in the editor.
    /// </summary>
    public bool HighlightCurrentLine
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <summary>
    ///     Whether to enable word wrap in the editor.
    /// </summary>
    public bool IsWordWrapEnabled
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    ///     Whether to synchronize the zoom level across all open editor tabs.
    /// </summary>
    public bool IsEditorZoomLevelSyncEnabled
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    ///     The font size used in the code editor.
    /// </summary>
    public int EditorFontSize
    {
        get;
        set => SetProperty(ref field, value);
    } = 14;

    /// <summary>
    ///     The font family used in the code editor. If null, the default font is used.
    /// </summary>
    public string? EditorFontFamily
    {
        get;
        set => SetProperty(ref field, value);
    } = null;

    /// <summary>
    ///     The number of spaces per tab in the code editor.
    /// </summary>
    public int EditorTabSize
    {
        get;
        set => SetProperty(ref field, value);
    } = 4;

    /// <summary>
    ///     Whether to use tabs for indentation in the code editor. If false, spaces are used.
    /// </summary>
    public bool UseEditorTabsForIndentation
    {
        get;
        set => SetProperty(ref field, value);
    }
}