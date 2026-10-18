using Avalonia;
using Avalonia.Controls;
using SkEditor.Analyzing.Models;

namespace SkEditor.Editor.UI.Controls;

public partial class AnalyzerTooltipView : UserControl
{
    public static readonly StyledProperty<AnalyzerError?> DiagnosticProperty =
        AvaloniaProperty.Register<Configuration.UI.Controls.ColorPickerSettingsExpander, AnalyzerError?>(nameof(Diagnostic));

    public AnalyzerError? Diagnostic
    {
        get => GetValue(DiagnosticProperty);
        set => SetValue(DiagnosticProperty, value);
    }

    public AnalyzerTooltipView() => InitializeComponent();
}