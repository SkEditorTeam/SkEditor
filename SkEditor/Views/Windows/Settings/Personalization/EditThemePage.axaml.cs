using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Styling;
using ColorPickerSettingsExpander = SkEditor.Views.Controls.ColorPickerSettingsExpander;

namespace SkEditor.Views.Windows.Settings.Personalization;

public partial class EditThemePage : UserControl
{
    public EditThemePage()
    {
        InitializeComponent();

        SetUpColorPickers();
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
        SetUpColorPicker(CurrentLineBackgroundExpander, "CurrentLineBackground");
        SetUpColorPicker(CurrentLineBorderExpander, "CurrentLineBorder");
        SetUpColorPicker(AccentColorExpander, "AccentColor");
    }

    private static void SetUpColorPicker(ColorPickerSettingsExpander expander, string propertyName)
    {
        ColorPickerButton? colorPicker = expander.ColorPicker;
        colorPicker.Color =
            (ThemeEditor.CurrentTheme.GetType().GetProperty(propertyName)?.GetValue(ThemeEditor.CurrentTheme) as
                ImmutableSolidColorBrush)?.Color;
        colorPicker.ColorChanged += async (_, e) => await ChangeColor(propertyName, e.NewColor);
    }

    private static async Task ChangeColor(string name, Color? color)
    {
        if (color is null)
        {
            return;
        }

        ThemeEditor.CurrentTheme.GetType().GetProperty(name)
            ?.SetValue(ThemeEditor.CurrentTheme, new ImmutableSolidColorBrush(color.Value));
        await ThemeEditor.ApplyTheme();
    }
}