using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using SkEditor.Utilities.Docs;

namespace SkEditor.Views.Pages;

public partial class DocumentationTabView : UserControl
{
    public DocumentationTabView()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        PatternEditor.TextArea.TextView.LineTransformers.Add(new DocumentationPatternColorizer());
        PatternEditor.TextArea.SelectionBrush =
            (ImmutableSolidColorBrush?)Application.Current?.FindResource("SelectionColor");
        PatternEditor.TextArea.CaretBrush = Brushes.Transparent;
        PatternEditor.Options.LineHeightFactor = 1.3;
    }

    public void InitializeComponentState()
    {
        OnLoaded(null, new RoutedEventArgs());
    }
}