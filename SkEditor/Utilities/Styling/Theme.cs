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

	public ImmutableSolidColorBrush BackgroundColor { get; set; } = new(Color.Parse("#1f1f1f"));
	public ImmutableSolidColorBrush SmallWindowBackgroundColor { get; set; } = new(Color.Parse("#1f1f1f"));
	public ImmutableSolidColorBrush EditorBackgroundColor { get; set; } = new(Color.Parse("#1b1b1b"));
	public ImmutableSolidColorBrush EditorTextColor { get; set; } = new(Color.Parse("#cdcaca"));
	public ImmutableSolidColorBrush LineNumbersColor { get; set; } = new(Color.Parse("#8a8a8a"));
	public ImmutableSolidColorBrush SelectionColor { get; set; } = new(Color.Parse("#641a6096"));
	public ImmutableSolidColorBrush SelectedTabItemBackground { get; set; } = new(Color.Parse("#282828"));
	public ImmutableSolidColorBrush SelectedTabItemBorder { get; set; } = new(Color.Parse("#751c1c1c"));
	public ImmutableSolidColorBrush MenuBackground { get; set; } = new(Color.Parse("#2c2c2c"));
	public ImmutableSolidColorBrush MenuBorder { get; set; } = new(Color.Parse("#75161616"));
	public ImmutableSolidColorBrush TextBoxFocusedBackground { get; set; } = new(Color.Parse("#B31E1E1E"));
	public ImmutableSolidColorBrush AccentColor { get; set; } = new(Color.Parse("#0ead6c"));
	public string? CustomFont { get; set; }

	public Dictionary<string, ImmutableSolidColorBrush> CustomColorChanges { get; set; } = new();
}