using Avalonia;
using Avalonia.Controls;

namespace SkEditor.Views.Controls;

public partial class ColorPickerSettingsExpander : UserControl
{
    public static readonly StyledProperty<string> ExpanderHeaderProperty =
        AvaloniaProperty.Register<ColorPickerSettingsExpander, string>(nameof(ExpanderHeader));

    public ColorPickerSettingsExpander()
    {
        InitializeComponent();

        DataContext = this;
    }

    public string ExpanderHeader
    {
        get => GetValue(ExpanderHeaderProperty);
        set => SetValue(ExpanderHeaderProperty, value);
    }

    public bool IsAlphaEnabled { get; set; } = true;
}