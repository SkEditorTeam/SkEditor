using Avalonia.Media;
using Avalonia.Media.Immutable;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SkEditor.Utilities.Styling;


public class Theme
{
    [JsonIgnore]
    public string FileName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ImmutableSolidColorBrush BackgroundColor { get; set; } = new(Color.Parse("#ff0d0d0d"));
    public ImmutableSolidColorBrush SmallWindowBackgroundColor { get; set; } = new(Color.Parse("#ff0f0f0f"));
    public ImmutableSolidColorBrush EditorBackgroundColor { get; set; } = new(Color.Parse("#ff0d0d0d"));
    public ImmutableSolidColorBrush EditorTextColor { get; set; } = new(Color.Parse("#ffcdcaca"));
    public ImmutableSolidColorBrush LineNumbersColor { get; set; } = new(Color.Parse("#ff8a8a8a"));
    public ImmutableSolidColorBrush SelectionColor { get; set; } = new(Color.Parse("#641a6096"));
    public ImmutableSolidColorBrush SelectedTabItemBackground { get; set; } = new(Color.Parse("#ff151515"));
    public ImmutableSolidColorBrush SelectedTabItemBorder { get; set; } = new(Color.Parse("#00181818"));
    public ImmutableSolidColorBrush MenuBackground { get; set; } = new(Color.Parse("#ff151515"));
    public ImmutableSolidColorBrush MenuBorder { get; set; } = new(Color.Parse("#ff262626"));
    public ImmutableSolidColorBrush TextBoxFocusedBackground { get; set; } = new(Color.Parse("#ff191919"));
    public ImmutableSolidColorBrush AccentColor { get; set; } = new(Colors.White);
    public string? CustomFont { get; set; }

    public Dictionary<string, ImmutableSolidColorBrush> CustomColorChanges { get; set; } = new()
    {
        { "TextControlSelectionHighlightColor", new(Color.Parse("#25ffffff")) },
        { "ContentDialogTopOverlay", new(Color.Parse("#ff141414")) },
        { "ToggleButtonBackgroundChecked", new(Color.Parse("#40ffffff")) },
        { "ToggleButtonBackgroundCheckedPointerOver", new(Color.Parse("#40ffffff")) }
    };
}