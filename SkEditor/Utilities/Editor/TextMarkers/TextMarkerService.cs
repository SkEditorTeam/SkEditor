using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using SkEditor.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkEditor.Utilities.Editor.TextMarkers;

public sealed class TextMarkerService(TextDocument document) : DocumentColorizingTransformer, IBackgroundRenderer,
    ITextMarkerService,
    ITextViewConnect
{
    private readonly TextDocument document = document ?? throw new ArgumentNullException(nameof(document));
    private readonly TextSegmentCollection<TextMarker> markers = new(document);

    public KnownLayer Layer => KnownLayer.Selection;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        ArgumentNullException.ThrowIfNull(textView);
        ArgumentNullException.ThrowIfNull(drawingContext);
        if (markers == null || !textView.VisualLinesValid)
            return;
        var visualLines = textView.VisualLines;
        if (visualLines.Count == 0)
            return;
        var viewStart = visualLines.First().FirstDocumentLine.Offset;
        var viewEnd = visualLines.Last().LastDocumentLine.EndOffset;
        foreach (var marker in markers.FindOverlappingSegments(viewStart, viewEnd - viewStart))
        {
            if (marker.BackgroundColor != null)
            {
                var geoBuilder = new BackgroundGeometryBuilder
                {
                    AlignToWholePixels = true,
                    CornerRadius = 3
                };
                geoBuilder.AddSegment(textView, marker);
                var geometry = geoBuilder.CreateGeometry();
                if (geometry != null)
                {
                    var color = marker.BackgroundColor.Value;
                    var brush = new SolidColorBrush(color);
                    drawingContext.DrawGeometry(brush, null, geometry);
                }
            }

            var underlineMarkerTypes = TextMarkerTypes.SquigglyUnderline | TextMarkerTypes.NormalUnderline;
            if ((marker.MarkerTypes & underlineMarkerTypes) != 0)
                foreach (var r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    var startPoint = r.BottomLeft;
                    var endPoint = r.BottomRight;

                    Brush usedBrush = new SolidColorBrush(marker.MarkerColor);
                    if ((marker.MarkerTypes & TextMarkerTypes.SquigglyUnderline) != 0)
                    {
                        var offset = 2.5;

                        var count = Math.Max((int) ((endPoint.X - startPoint.X) / offset) + 1, 0);

                        var geometry = new StreamGeometry();

                        using (var ctx = geometry.Open())
                        {
                            var points = CreatePoints(startPoint, endPoint, offset, count).ToArray();

                            ctx.BeginFigure(points[0], false);

                            for (var i = 0; i < points.Length; i++)
                                if (i + 1 < points.Length)
                                    ctx.QuadraticBezierTo(points[i], new Point((points[i].X + points[i + 1].X) / 2,
                                        (points[i].Y + points[i + 1].Y) / 2));
                        }

                        var fontSize = ApiVault.Get().GetTextEditor().FontSize;
                        var strokeThickness = (float) Math.Max(fontSize / 15, 1);

                        Pen usedPen = new(usedBrush, strokeThickness);
                        drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
                    }

                    if ((marker.MarkerTypes & TextMarkerTypes.NormalUnderline) != 0)
                    {
                        var usedPen = new Pen(usedBrush);
                        drawingContext.DrawLine(usedPen, startPoint, endPoint);
                    }
                }
        }
    }

    public ITextMarker Create(int startOffset, int length)
    {
        if (markers == null) return null;

        var textLength = document.TextLength;
        if (startOffset < 0 || startOffset > textLength
                            || length < 0 || startOffset + length > textLength) return null;

        TextMarker marker = new(this, startOffset, length);
        markers.Add(marker);
        Redraw(marker);
        return marker;
    }

    public IEnumerable<ITextMarker> GetMarkersAtOffset(int offset)
    {
        return markers?.FindSegmentsContaining(offset) ?? Enumerable.Empty<ITextMarker>();
    }

    public IEnumerable<ITextMarker> TextMarkers => markers ?? Enumerable.Empty<ITextMarker>();

    public void RemoveAll()
    {
        markers.ToList().ToList().ForEach(Remove);
    }

    public void Remove(ITextMarker marker)
    {
        if (marker == null) return;

        var m = marker as TextMarker;

        markers.Remove(m);
        Redraw(m);
    }

    public void Redraw(ISegment segment)
    {
        RedrawRequested?.Invoke(this, EventArgs.Empty);
        ApiVault.Get().GetTextEditor().TextArea.TextView.Redraw(segment);
    }

    public event EventHandler RedrawRequested;

    protected override void ColorizeLine(DocumentLine line)
    {
    }

    private static IEnumerable<Point> CreatePoints(Point start, Point end, double offset, int count)
    {
        var fontSize = ApiVault.Get().GetTextEditor().FontSize;
        var multiplier = fontSize * 0.075;


        for (var i = 0; i < count; i++)
        {
            var yOffset = (i + 1) % 2 == 0 ? offset : 0;
            yOffset -= 2.5;
            if (start.X + i * offset * multiplier > end.X)
            {
                yield return end;
                yield break;
            }

            yield return new Point(start.X + i * offset * multiplier, start.Y + yOffset * multiplier);
        }
    }
}

public sealed class TextMarker : TextSegment, ITextMarker
{
    private readonly TextMarkerService service;

    public TextMarker(TextMarkerService service, int startOffset, int length)
    {
        this.service = service ?? throw new ArgumentNullException(nameof(service));
        StartOffset = startOffset;
        Length = length;
        MarkerTypes = TextMarkerTypes.None;
    }

    public bool IsDeleted => !IsConnectedToCollection;

    public Color? BackgroundColor { get; set; }
    public Color? ForegroundColor { get; set; }
    public TextMarkerTypes MarkerTypes { get; set; }
    public Color MarkerColor { get; set; }

    public Func<UserControl> Tooltip { get; set; }

    public void Delete()
    {
        service.Remove(this);
    }
}