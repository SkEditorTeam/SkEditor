using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Controls;
using SkEditor.Utilities.Styling;

namespace SkEditor.Views.Settings;
public partial class EditThemePage : UserControl
{
    public EditThemePage()
    {
        InitializeComponent();

        AssignCommands();
        SetUpColorPickers();
    }

    private void AssignCommands()
    {
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(ThemePage)));
    }

    private void SetUpColorPickers()
    {
        SetUpColorPicker(WindowBackgroundExpander, "BackgroundColor");
        SetUpColorPicker(SmallWindowBackgroundExpander, "SmallWindowBackgroundColor");
        SetUpColorPicker(EditorBackgroundExpander, "EditorBackgroundColor");
        SetUpColorPicker(EditorForegroundExpander, "EditorTextColor");
        SetUpColorPicker(LineNumbersExpander, "LineNumbersColor");
        SetUpColorPicker(SelectionExpander, "SelectionColor");
        SetUpColorPicker(SelectedTabItemBackgroundExpander, "SelectedTabItemBackground");
        SetUpColorPicker(SelectedTabItemBorderExpander, "SelectedTabItemBorder");
        SetUpColorPicker(MenuBackgroundExpander, "MenuBackground");
        SetUpColorPicker(MenuBorderExpander, "MenuBorder");
        SetUpColorPicker(TextBoxFocusedBackgroundExpander, "TextBoxFocusedBackground");
        SetUpColorPicker(AccentColorExpander, "AccentColor");
    }

    private static void SetUpColorPicker(ColorPickerSettingsExpander expander, string propertyName)
    {
        var colorPicker = expander.ColorPicker;
        colorPicker.Color = (ThemeEditor.CurrentTheme.GetType().GetProperty(propertyName)?.GetValue(ThemeEditor.CurrentTheme) as ImmutableSolidColorBrush)?.Color;
        colorPicker.ColorChanged += (s, e) => ChangeColor(propertyName, e.NewColor);
    }

    private static void ChangeColor(string name, Color? color)
    {
        if (color is null) return;

        ThemeEditor.CurrentTheme.GetType().GetProperty(name)?.SetValue(ThemeEditor.CurrentTheme, new ImmutableSolidColorBrush(color.Value));
        ThemeEditor.ApplyTheme();
    }
}
