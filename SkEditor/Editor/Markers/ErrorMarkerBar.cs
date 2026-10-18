using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using ObservableCollections;
using R3;
using SkEditor.Models.Analyzer;

namespace SkEditor.Views.Controls.Editor;

public class ErrorMarkerBar : Control
{
    public static readonly StyledProperty<TextEditor?> EditorProperty =
        AvaloniaProperty.Register<ErrorMarkerBar, TextEditor?>(nameof(Editor));

    public static readonly StyledProperty<ObservableList<AnalyzerError>?> DiagnosticsProperty =
        AvaloniaProperty.Register<ErrorMarkerBar, ObservableList<AnalyzerError>?>(nameof(Diagnostics));

    public static readonly StyledProperty<Thickness> TrackPaddingProperty =
        AvaloniaProperty.Register<ErrorMarkerBar, Thickness>(
            nameof(TrackPadding),
            new Thickness(0, 2, 0, 2)
        );

    private const double MarkerHeight = 3.0;
    private const double MarkerWidth = 12.0;
    private IDisposable? _diagnosticsSubscription;

    private IDisposable? _editorSubscription;
    private ScrollBar? _verticalScrollBar;

    public ObservableList<AnalyzerError>? Diagnostics
    {
        get => GetValue(DiagnosticsProperty);
        set => SetValue(DiagnosticsProperty, value);
    }

    public TextEditor? Editor
    {
        get => GetValue(EditorProperty);
        set => SetValue(EditorProperty, value);
    }

    public Thickness TrackPadding
    {
        get => GetValue(TrackPaddingProperty);
        set => SetValue(TrackPaddingProperty, value);
    }

    static ErrorMarkerBar() => AffectsRender<ErrorMarkerBar>(EditorProperty, DiagnosticsProperty);

    public ErrorMarkerBar() => Width = MarkerWidth;

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Editor?.Document == null || Diagnostics == null || Diagnostics.Count == 0)
        {
            return;
        }

        TextView? textView = Editor.TextArea.TextView;
        if (textView == null)
        {
            return;
        }

        if (textView.DocumentHeight <= textView.Bounds.Height)
        {
            return;
        }

        var groupedMarkers = Diagnostics
            .Select(error => new
            {
                Y = Math.Round(CalculateYPosition(error.Line)),
                Error = error
            })
            .GroupBy(m => m.Y)
            .Select(g => new
            {
                Y = g.Key,
                HighestSeverity = g.Min(x => x.Error.Type)
            });

        CacheScrollBar();

        foreach (var marker in groupedMarkers)
        {
            Color color = marker.HighestSeverity switch
            {
                AnalyzerErrorType.Error => Color.FromArgb(255, 237, 27, 60),
                AnalyzerErrorType.Warning => Color.FromArgb(255, 252, 172, 0),
                AnalyzerErrorType.Info => Color.FromArgb(255, 26, 144, 237),
                _ => Colors.Transparent
            };

            SolidColorBrush brush = new(color);
            context.DrawRectangle(brush, null, new Rect(0, marker.Y, MarkerWidth, MarkerHeight), 1.5, 1.5);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EditorProperty)
        {
            _editorSubscription?.Dispose();

            if (change.GetNewValue<TextEditor?>() is { } editor)
            {
                editor.TextArea.TextView.VisualLinesChanged += OnVisualLinesChanged;

                _editorSubscription = Disposable.Create(() =>
                {
                    editor.TextArea.TextView.VisualLinesChanged -= OnVisualLinesChanged;
                });
            }

            InvalidateVisual();
        }

        else if (change.Property == DiagnosticsProperty)
        {
            _diagnosticsSubscription?.Dispose();

            if (change.GetNewValue<ObservableList<AnalyzerError>?>() is { } newList)
            {
                _diagnosticsSubscription = newList
                    .ObserveChanged()
                    .Subscribe(_ => InvalidateVisual());
            }

            InvalidateVisual();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _diagnosticsSubscription?.Dispose();
    }

    private void OnVisualLinesChanged(object? sender, EventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }

        InvalidateVisual();
    }

    private void CacheScrollBar()
    {
        if (Editor?.ScrollViewer != null && _verticalScrollBar == null)
        {
            _verticalScrollBar = Editor.ScrollViewer
                .GetVisualDescendants()
                .OfType<ScrollBar>()
                .FirstOrDefault(s => s.Orientation == Orientation.Vertical);
        }
    }

    private double CalculateYPosition(int lineNumber)
    {
        if (Editor?.Document == null)
        {
            return TrackPadding.Top;
        }

        int totalLines = Editor.Document.LineCount;
        if (totalLines <= 0)
        {
            return TrackPadding.Top;
        }

        int clampedLine = Math.Clamp(lineNumber, 1, totalLines);

        TextView? textView = Editor.TextArea.TextView;
        if (textView == null)
        {
            return TrackPadding.Top;
        }

        double totalExtent = ((IScrollable)textView).Extent.Height;
        if (totalExtent <= 0)
        {
            totalExtent = textView.DocumentHeight;
        }

        if (totalExtent <= 0)
        {
            return TrackPadding.Top;
        }

        double topPadding = TrackPadding.Top;
        double bottomPadding = TrackPadding.Bottom;

        double usableHeight = Math.Max(0, Bounds.Height - topPadding - bottomPadding - MarkerHeight);

        double visualTop = textView.GetVisualTopByDocumentLine(clampedLine);
        double ratio = Math.Clamp(visualTop / totalExtent, 0.0, 1.0);

        double y = topPadding + (ratio * usableHeight);
        return y;
    }
}