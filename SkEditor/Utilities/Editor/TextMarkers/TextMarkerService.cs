using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Utilities.Editor.TextMarkers;

public sealed class TextMarkerService(FileTabViewModel fileTab) : DocumentColorizingTransformer, IBackgroundRenderer,
    ITextMarkerService
{
    private readonly TextDocument _document =
        fileTab.Document ?? throw new ArgumentNullException(nameof(fileTab.Document));

    private readonly TextSegmentCollection<TextMarker> _markers = new(fileTab.Document);

    public KnownLayer Layer => KnownLayer.Selection;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        ArgumentNullException.ThrowIfNull(textView);
        ArgumentNullException.ThrowIfNull(drawingContext);
        if (_markers == null || !textView.VisualLinesValid)
        {
            return;
        }

        ReadOnlyCollection<VisualLine>? visualLines = textView.VisualLines;
        if (visualLines.Count == 0)
        {
            return;
        }

        int viewStart = visualLines.First().FirstDocumentLine.Offset;
        int viewEnd = visualLines.Last().LastDocumentLine.EndOffset;
        foreach (TextMarker? marker in _markers.FindOverlappingSegments(viewStart, viewEnd - viewStart))
        {
            if (marker.BackgroundColor != null)
            {
                BackgroundGeometryBuilder geoBuilder = new()
                {
                    AlignToWholePixels = true,
                    CornerRadius = 3
                };
                geoBuilder.AddSegment(textView, marker);
                Geometry? geometry = geoBuilder.CreateGeometry();
                if (geometry != null)
                {
                    Color color = marker.BackgroundColor.Value;
                    SolidColorBrush brush = new(color);
                    drawingContext.DrawGeometry(brush, null, geometry);
                }
            }

            const TextMarkerTypes underlineMarkerTypes =
                TextMarkerTypes.SquigglyUnderline | TextMarkerTypes.NormalUnderline;
            if ((marker.MarkerTypes & underlineMarkerTypes) == 0)
            {
                continue;
            }

            foreach (Rect r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
            {
                Point startPoint = r.BottomLeft;
                Point endPoint = r.BottomRight;

                Brush usedBrush = new SolidColorBrush(marker.MarkerColor);
                if ((marker.MarkerTypes & TextMarkerTypes.SquigglyUnderline) != 0)
                {
                    const double offset = 2.5;

                    int count = Math.Max((int)((endPoint.X - startPoint.X) / offset) + 1, 0);

                    StreamGeometry geometry = new();

                    using StreamGeometryContext ctx = geometry.Open();
                    Point[] points = CreatePoints(startPoint, endPoint, offset, count).ToArray();

                    ctx.BeginFigure(points[0], false);

                    for (int i = 0; i < points.Length; i++)
                    {
                        if (i + 1 < points.Length)
                        {
                            ctx.QuadraticBezierTo(points[i], new Point((points[i].X + points[i + 1].X) / 2,
                                (points[i].Y + points[i + 1].Y) / 2));
                        }
                    }

                    double fontSize = fileTab.FontSize;

                    double strokeThickness = Math.Max(fontSize / 15, 1);

                    Pen usedPen = new(usedBrush, strokeThickness);
                    drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
                }

                if ((marker.MarkerTypes & TextMarkerTypes.NormalUnderline) != 0)
                {
                    Pen usedPen = new(usedBrush);
                    drawingContext.DrawLine(usedPen, startPoint, endPoint);
                }
            }
        }
    }

    public ITextMarker? Create(int startOffset, int length)
    {
        if (_markers == null)
        {
            return null;
        }

        int textLength = _document.TextLength;
        if (startOffset < 0 || startOffset > textLength
                            || length < 0 || startOffset + length > textLength)
        {
            return null;
        }

        TextMarker marker = new(this, startOffset, length);
        _markers.Add(marker);
        Redraw(marker);
        return marker;
    }

    public IEnumerable<ITextMarker> GetMarkersAtOffset(int offset)
    {
        return _markers?.FindSegmentsContaining(offset) ?? Enumerable.Empty<ITextMarker>();
    }

    public IEnumerable<ITextMarker> TextMarkers => _markers;

    public void RemoveAll()
    {
        _markers.ToList().ForEach(Remove);
    }

    public void Remove(ITextMarker marker)
    {
        if (marker is not TextMarker m || m.IsDeleted)
        {
            return;
        }

        _markers.Remove(m);
        Redraw(m);
    }

    public void Redraw(ISegment segment)
    {
        RedrawRequested?.Invoke(this, EventArgs.Empty);
        fileTab.RequestRedraw(segment);
    }

    public event EventHandler? RedrawRequested;

    protected override void ColorizeLine(DocumentLine line)
    {
    }

    private IEnumerable<Point> CreatePoints(Point start, Point end, double offset, int count)
    {
        double multiplier = fileTab.FontSize * 0.075;

        for (int i = 0; i < count; i++)
        {
            double yOffset = (i + 1) % 2 == 0 ? offset : 0;
            yOffset -= 2.5;
            if (start.X + (i * offset * multiplier) > end.X)
            {
                yield return end;
                yield break;
            }

            yield return new Point(start.X + (i * offset * multiplier), start.Y + (yOffset * multiplier));
        }
    }
}