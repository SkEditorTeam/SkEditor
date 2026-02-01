using System.Collections.Generic;
using System.Text.Json.Serialization;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using SkEditor.Utilities.Styling.Attributes;

namespace SkEditor.Utilities.Styling;

[Theme]
public partial class Theme
{
    [JsonIgnore] public string FileName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ImmutableSolidColorBrush BackgroundColor { get; set; } = new(Color.Parse("#ff0d0d0d"));
    public ImmutableSolidColorBrush SmallWindowBackgroundColor { get; set; } = new(Color.Parse("#ff0f0f0f"));
    public ImmutableSolidColorBrush DialogTopBackgroundColor { get; set; } = new(Color.Parse("#121212"));
    public ImmutableSolidColorBrush DialogTopBorderColor { get; set; } = new(Color.Parse("#75161616"));
    public ImmutableSolidColorBrush DialogBackgroundColor { get; set; } = new(Color.Parse("#ff0d0d0d"));
    public ImmutableSolidColorBrush EditorBackgroundColor { get; set; } = new(Color.Parse("#ff0d0d0d"));
    public ImmutableSolidColorBrush EditorTextColor { get; set; } = new(Color.Parse("#ffcdcaca"));
    public ImmutableSolidColorBrush LineNumbersColor { get; set; } = new(Color.Parse("#ff8a8a8a"));
    public ImmutableSolidColorBrush SelectionColor { get; set; } = new(Color.Parse("#641a6096"));
    public ImmutableSolidColorBrush SelectedTabItemBackground { get; set; } = new(Color.Parse("#ff161616"));
    public ImmutableSolidColorBrush SelectedTabItemBorder { get; set; } = new(Color.Parse("#00181818"));
    public ImmutableSolidColorBrush MenuBackground { get; set; } = new(Color.Parse("#ff151515"));
    public ImmutableSolidColorBrush MenuBorder { get; set; } = new(Color.Parse("#ff262626"));
    public ImmutableSolidColorBrush TextBoxFocusedBackground { get; set; } = new(Color.Parse("#ff191919"));
    public ImmutableSolidColorBrush CurrentLineBackground { get; set; } = new(Color.Parse("#05FFFFFF"));
    public ImmutableSolidColorBrush CurrentLineBorder { get; set; } = new(Color.Parse("#0DFFFFFF"));
    public ImmutableSolidColorBrush AccentColor { get; set; } = new(Colors.White);
    public string? CustomFont { get; set; }
    public bool UseMicaEffect { get; set; }

    public Dictionary<string, ImmutableSolidColorBrush> CustomColorChanges { get; set; } = new()
    {
        { "TextControlSelectionHighlightColor", new ImmutableSolidColorBrush(Color.Parse("#25ffffff")) },
        { "ContentDialogTopOverlay", new ImmutableSolidColorBrush(Color.Parse("#ff141414")) },
        { "ToggleButtonBackgroundChecked", new ImmutableSolidColorBrush(Color.Parse("#40ffffff")) },
        { "ToggleButtonBackgroundCheckedPointerOver", new ImmutableSolidColorBrush(Color.Parse("#40ffffff")) }
    };
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    AllowTrailingCommas = true,
    Converters = [typeof(ImmutableSolidColorBrushConverter)]
)]
[JsonSerializable(typeof(Theme))]
public partial class ThemeJsonContext : JsonSerializerContext;