using Avalonia;
using Avalonia.Controls;
using SkEditor.Models.Analyzer;

namespace SkEditor.Views.Controls.Editor;

public partial class AnalyzerTooltipView : UserControl
{
    public static readonly StyledProperty<AnalyzerError?> DiagnosticProperty =
        AvaloniaProperty.Register<ColorPickerSettingsExpander, AnalyzerError?>(nameof(Diagnostic));

    public AnalyzerError? Diagnostic
    {
        get => GetValue(DiagnosticProperty);
        set => SetValue(DiagnosticProperty, value);
    }

    public AnalyzerTooltipView() => InitializeComponent();
}