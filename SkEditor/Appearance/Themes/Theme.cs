using System.Collections.Generic;
using System.Text.Json.Serialization;
using Avalonia.Media;
using SkEditor.Appearance.Converters;
using SkEditor.Appearance.Themes.Attributes;
using Brush = Avalonia.Media.Immutable.ImmutableSolidColorBrush;

namespace SkEditor.Appearance.Themes;

[Theme]
public partial class Theme
{
    [ThemeResource]
    public Brush? AccentColor { get; set; } = C("#FFFFFF");

    [ThemeResource]
    public Brush BackgroundColor { get; set; } = C("#ff0d0d0d");

    [ThemeResource]
    public Brush CurrentLineBackground { get; set; } = C("#05FFFFFF");

    [ThemeResource]
    public Brush CurrentLineBorder { get; set; } = C("#0DFFFFFF");

    public Dictionary<string, Brush> CustomColorChanges { get; set; } = new()
    {
        { "TextControlSelectionHighlightColor", C("#25ffffff") },
        { "ContentDialogTopOverlay", C("#ff141414") },
        { "ToggleButtonBackgroundChecked", C("#40ffffff") },
        { "ToggleButtonBackgroundCheckedPointerOver", C("#40ffffff") }
    };

    public string? CustomFont { get; set; }

    [ThemeResource]
    public Brush DialogBackgroundColor { get; set; } = C("#ff0d0d0d");

    [ThemeResource]
    public Brush EditorBackgroundColor { get; set; } = C("#ff0d0d0d");

    [ThemeResource]
    public Brush EditorTextColor { get; set; } = C("#ffcdcaca");

    public Dictionary<string, string> ExplorerIconColors { get; set; } = new()
    {
        { "sftp-root", "#8CA9FF" },
        { "folder", "#FFCF40" },
        { "skript", "#00C853" },
        { "database", "#BEF26F" },
        { "table", "#FF8A30" },
        { "yaml", "#49B7E3" },
        { "json", "#FFB300" },
        { "settings", "#49B7E3" },
        { "xml", "#44D468" },
        { "schematic", "#FF9800" },
        { "nbt", "#607D8B" },
        { "image", "#29B6F6" },
        { "audio", "#BCF02E" },
        { "video", "#FF5252" },
        { "text", "#CFD8DC" },
        { "log", "#BDCFD9" },
        { "markdown", "#4FC3F7" },
        { "binary", "#F53D3D" },
        { "executable", "#35D4B4" },
        { "jar", "#F03765" },
        { "class", "#D32F2F" },
        { "shell", "#4AB9ED" },
        { "archive", "#FF87B7" },
        { "key", "#E38B2D" },
        { "git", "#F26A3D" },
        { "pdf", "#FF595C" },
        { "code", "#7ED4E6" }
    };

    [JsonIgnore]
    public string FileName { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsBuiltIn { get; set; }

    [ThemeResource]
    public Brush LineNumbersColor { get; set; } = C("#ff8a8a8a");

    [ThemeResource("MenuFlyoutPresenterBackground", "ComboBoxDropDownBackground", "FlyoutPresenterBackground",
        "SearchPanelBackgroundBrush",
        MapPropertyName = false)]
    public Brush MenuBackground { get; set; } = C("#ff151515");

    [ThemeResource("MenuFlyoutPresenterBorderBrush", "ComboBoxDropDownBorderBrush", "FlyoutBorderThemeBrush",
        "SearchPanelBorderBrush",
        MapPropertyName = false)]
    public Brush MenuBorder { get; set; } = C("#ff262626");

    public string Name { get; set; } = string.Empty;

    [ThemeResource]
    public Brush NotificationBackground { get; set; } = C("#ff151515");

    [ThemeResource]
    public Brush NotificationBorderBrush { get; set; } = C("#ff262626");

    [ThemeResource("TabViewItemHeaderBackgroundSelected", MapPropertyName = false)]
    public Brush SelectedTabItemBackground { get; set; } = C("#ff161616");

    [ThemeResource("TabViewSelectedItemBorderBrush", MapPropertyName = false)]
    public Brush SelectedTabItemBorder { get; set; } = C("#00181818");

    [ThemeResource]
    public Brush SelectionColor { get; set; } = C("#641a6096");

    [ThemeResource("TaskDialogButtonAreaBackground")]
    public Brush SmallWindowBackgroundColor { get; set; } = C("#ff0f0f0f");

    [ThemeResource]
    public Brush SnippetActiveBackground { get; set; } = C("#336587FF");

    [ThemeResource]
    public Brush SnippetActiveBorder { get; set; } = C("#FF6587FF");

    public Dictionary<string, string> TerminalColors { get; set; } = new();

    [ThemeResource("TextControlBackgroundFocused", MapPropertyName = false)]
    public Brush TextBoxFocusedBackground { get; set; } = C("#ff191919");

    public bool UseMicaEffect { get; set; }

    private static Brush C(string hex)
    {
        return new Brush(Color.Parse(hex));
    }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    AllowTrailingCommas = true,
    Converters = [typeof(ImmutableSolidColorBrushConverter)]
)]
[JsonSerializable(typeof(Theme))]
public partial class ThemeJsonContext : JsonSerializerContext;