using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SkEditor.Controls;

public partial class SettingsCategorySeparator : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SettingsCategorySeparator, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public SettingsCategorySeparator()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        TitleBlock.Text = Title;
    }
}