using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using R3;
using SkEditor.Contracts.Services;
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
            new Thickness(0, 12, 0, 12)
        );

    private IDisposable? _diagnosticsSubscription;

    static ErrorMarkerBar() => AffectsRender<ErrorMarkerBar>(EditorProperty, DiagnosticsProperty);

    public TextEditor? Editor
    {
        get => GetValue(EditorProperty);
        set => SetValue(EditorProperty, value);
    }

    public ObservableList<AnalyzerError>? Diagnostics
    {
        get => GetValue(DiagnosticsProperty);
        set => SetValue(DiagnosticsProperty, value);
    }

    public Thickness TrackPadding
    {
        get => GetValue(TrackPaddingProperty);
        set => SetValue(TrackPaddingProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DiagnosticsProperty)
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

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Editor?.Document == null || Diagnostics == null || Diagnostics.Count == 0)
        {
            return;
        }

        double totalHeight = Bounds.Height;
        int totalLines = Editor.Document.LineCount;

        if (totalLines == 0 || totalHeight <= 0)
        {
            return;
        }

        App.Services.GetRequiredService<ILogService>().Debug(
            $"document height: {Editor.TextArea.TextView.DocumentHeight}, bounds: {Editor.TextArea.TextView.Bounds.Height}, height: {Editor.TextArea.TextView.Height}");
        if (Editor.TextArea.TextView.DocumentHeight <= Editor.TextArea.TextView.Bounds.Height)
        {
            return;
        }

        var groupedMarkers = Diagnostics
            .Select(err => new
            {
                Y = Math.Round(CalculateYPosition(err.Line) / 2.0) * 2.0,
                Error = err
            })
            .GroupBy(x => x.Y)
            .Select(g => new
            {
                Y = g.Key,
                HighestSeverity = g.Min(x => x.Error.Type)
            });

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
            context.DrawRectangle(brush, null, new Rect(0, marker.Y, Bounds.Width, 3), 1.5, 1.5);
        }
    }

    private double CalculateYPosition(int lineNumber)
    {
        if (Editor?.Document == null || Editor.TextArea.TextView.DocumentHeight <= 0)
        {
            return 0;
        }

        TextView? textView = Editor.TextArea.TextView;
        double visualTop = textView.GetVisualTopByDocumentLine(lineNumber);
        double documentHeight = textView.DocumentHeight;

        double totalExtent = documentHeight;

        if (Editor.Options.AllowScrollBelowDocument)
        {
            double viewportHeight = textView.Bounds.Height;
            double defaultLineHeight = textView.DefaultLineHeight;

            double desiredHeight = Math.Min(viewportHeight, documentHeight);
            double extraHeightToAllowScrollBelowDocument = Math.Max(0, desiredHeight - defaultLineHeight);

            totalExtent = documentHeight + extraHeightToAllowScrollBelowDocument;
        }

        double topPadding = TrackPadding.Top;
        double bottomPadding = TrackPadding.Bottom;
        double usableHeight = Bounds.Height - topPadding - bottomPadding;

        if (usableHeight <= 0 || totalExtent <= 0)
        {
            return topPadding;
        }

        double y = topPadding + (visualTop / totalExtent * usableHeight);
        return y;
    }
}