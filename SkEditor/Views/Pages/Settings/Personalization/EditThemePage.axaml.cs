using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using SkEditor.Services;
using ColorPickerSettingsExpander = SkEditor.Views.Controls.ColorPickerSettingsExpander;
using SkEditorTheme = SkEditor.Utilities.Styling.Theme;

namespace SkEditor.Views.Pages.Settings.Personalization;

public partial class EditThemePage : UserControl
{
    public EditThemePage()
    {
        InitializeComponent();

        SetUpColorPickers();
    }

    private void SetUpColorPickers()
    {
        SetUpColorPicker(WindowBackgroundExpander, nameof(SkEditorTheme.BackgroundColor));
        SetUpColorPicker(SmallWindowBackgroundExpander, nameof(SkEditorTheme.SmallWindowBackgroundColor));
        SetUpColorPicker(EditorBackgroundExpander, nameof(SkEditorTheme.EditorBackgroundColor));
        SetUpColorPicker(EditorForegroundExpander, nameof(SkEditorTheme.EditorTextColor));
        SetUpColorPicker(LineNumbersExpander, nameof(SkEditorTheme.LineNumbersColor));
        SetUpColorPicker(SelectionExpander, nameof(SkEditorTheme.SelectionColor));
        SetUpColorPicker(SelectedTabItemBackgroundExpander, nameof(SkEditorTheme.SelectedTabItemBackground));
        SetUpColorPicker(SelectedTabItemBorderExpander, nameof(SkEditorTheme.SelectedTabItemBorder));
        SetUpColorPicker(MenuBackgroundExpander, nameof(SkEditorTheme.MenuBackground));
        SetUpColorPicker(MenuBorderExpander, nameof(SkEditorTheme.MenuBorder));
        SetUpColorPicker(TextBoxFocusedBackgroundExpander, nameof(SkEditorTheme.TextBoxFocusedBackground));
        SetUpColorPicker(CurrentLineBackgroundExpander, nameof(SkEditorTheme.CurrentLineBackground));
        SetUpColorPicker(CurrentLineBorderExpander, nameof(SkEditorTheme.CurrentLineBorder));
        SetUpColorPicker(SidebarIconForegroundExpander, nameof(SkEditorTheme.SidebarIconForeground));
        SetUpColorPicker(SidebarIconSelectedForegroundExpander, nameof(SkEditorTheme.SidebarIconSelectedForeground));
        SetUpColorPicker(AccentColorExpander, nameof(SkEditorTheme.AccentColor));
    }

    private static void SetUpColorPicker(ColorPickerSettingsExpander expander, string propertyName)
    {
        ColorPicker? colorPicker = expander.ColorPicker;
        colorPicker.Color = ThemeService.ServiceStatic.CurrentTheme.GetBrushByName(propertyName).Color;
        colorPicker.ColorChanged += async (_, e) => await ChangeColor(propertyName, e.NewColor);
    }

    private static async Task ChangeColor(string name, Color? color)
    {
        if (color is null)
        {
            return;
        }

        ThemeService.ServiceStatic.CurrentTheme.SetBrush(name, new ImmutableSolidColorBrush(color.Value));
        await ThemeService.ServiceStatic.ApplyTheme();
    }
}