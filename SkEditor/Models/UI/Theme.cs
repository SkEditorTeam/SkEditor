using System.Collections.Generic;
using System.Text.Json.Serialization;
using Avalonia.Media;
using SkEditor.Converters;
using SkEditor.Utilities.Styling.Attributes;
using Brush = Avalonia.Media.Immutable.ImmutableSolidColorBrush;

namespace SkEditor.Models.UI;

[Theme]
public partial class Theme
{
    [JsonIgnore]
    public string FileName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    [ThemeResource]
    public Brush BackgroundColor { get; set; } = C("#ff0d0d0d");

    [ThemeResource("ContentDialogBackground", "TaskDialogButtonAreaBackground")]
    public Brush SmallWindowBackgroundColor { get; set; } = C("#ff0f0f0f");

    [ThemeResource]
    public Brush DialogTopBackgroundColor { get; set; } = C("#121212");

    [ThemeResource]
    public Brush DialogTopBorderColor { get; set; } = C("#75161616");

    [ThemeResource]
    public Brush DialogBackgroundColor { get; set; } = C("#ff0d0d0d");

    [ThemeResource]
    public Brush EditorBackgroundColor { get; set; } = C("#ff0d0d0d");

    [ThemeResource]
    public Brush EditorTextColor { get; set; } = C("#ffcdcaca");

    [ThemeResource]
    public Brush LineNumbersColor { get; set; } = C("#ff8a8a8a");

    [ThemeResource]
    public Brush SelectionColor { get; set; } = C("#641a6096");

    [ThemeResource("TabViewItemHeaderBackgroundSelected", MapPropertyName = false)]
    public Brush SelectedTabItemBackground { get; set; } = C("#ff161616");

    [ThemeResource("TabViewSelectedItemBorderBrush", MapPropertyName = false)]
    public Brush SelectedTabItemBorder { get; set; } = C("#00181818");

    [ThemeResource("MenuFlyoutPresenterBackground", "ComboBoxDropDownBackground", "FlyoutPresenterBackground",
        "SearchPanelBackgroundBrush",
        MapPropertyName = false)]
    public Brush MenuBackground { get; set; } = C("#ff151515");

    [ThemeResource("MenuFlyoutPresenterBorderBrush", "ComboBoxDropDownBorderBrush", "FlyoutBorderThemeBrush",
        "SearchPanelBorderBrush",
        MapPropertyName = false)]
    public Brush MenuBorder { get; set; } = C("#ff262626");

    [ThemeResource("TextControlBackgroundFocused", MapPropertyName = false)]
    public Brush TextBoxFocusedBackground { get; set; } = C("#ff191919");

    [ThemeResource]
    public Brush CurrentLineBackground { get; set; } = C("#05FFFFFF");

    [ThemeResource]
    public Brush CurrentLineBorder { get; set; } = C("#0DFFFFFF");

    [ThemeResource]
    public Brush SidebarIconForeground { get; set; } = C("#FFCCCCCC");

    [ThemeResource]
    public Brush SidebarIconSelectedForeground { get; set; } = C("#FF60CDFF");

    [ThemeResource]
    public Brush AccentColor { get; set; } = C("#FFFFFF");

    public string? CustomFont { get; set; }
    public bool UseMicaEffect { get; set; }

    public Dictionary<string, Brush> CustomColorChanges { get; set; } = new()
    {
        { "TextControlSelectionHighlightColor", C("#25ffffff") },
        { "ContentDialogTopOverlay", C("#ff141414") },
        { "ToggleButtonBackgroundChecked", C("#40ffffff") },
        { "ToggleButtonBackgroundCheckedPointerOver", C("#40ffffff") }
    };

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