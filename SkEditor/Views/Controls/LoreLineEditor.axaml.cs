using Avalonia;
using Avalonia.Controls;
using SkEditor.Views.Windows.Generators.Gui;

namespace SkEditor.Views.Controls;

public partial class LoreLineEditor : UserControl
{
    public static readonly StyledProperty<bool> IsDeleteButtonVisibleProperty =
        AvaloniaProperty.Register<LoreLineEditor, bool>(nameof(IsDeleteButtonVisible));

    public LoreLineEditor()
    {
        InitializeComponent();

        DataContext = this;
        ColoredTextHandler.SetupBox(LineTextBox);
    }

    public bool IsDeleteButtonVisible
    {
        get => GetValue(IsDeleteButtonVisibleProperty);
        set => SetValue(IsDeleteButtonVisibleProperty, value);
    }
}