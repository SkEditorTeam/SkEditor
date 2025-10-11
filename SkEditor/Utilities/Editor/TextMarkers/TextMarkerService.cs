using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using SkEditor.API;

namespace SkEditor.Utilities.Editor.TextMarkers;

public sealed class TextMarkerService(TextDocument document) : DocumentColorizingTransformer, IBackgroundRenderer,
    ITextMarkerService
{
    private readonly TextDocument _document = document ?? throw new ArgumentNullException(nameof(document));
    private readonly TextSegmentCollection<TextMarker> _markers = new(document);

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

            TextMarkerTypes underlineMarkerTypes = TextMarkerTypes.SquigglyUnderline | TextMarkerTypes.NormalUnderline;
            if ((marker.MarkerTypes & underlineMarkerTypes) != 0)
            {
                foreach (Rect r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    Point startPoint = r.BottomLeft;
                    Point endPoint = r.BottomRight;

                    Brush usedBrush = new SolidColorBrush(marker.MarkerColor);
                    if ((marker.MarkerTypes & TextMarkerTypes.SquigglyUnderline) != 0)
                    {
                        double offset = 2.5;

                        int count = Math.Max((int)((endPoint.X - startPoint.X) / offset) + 1, 0);

                        StreamGeometry geometry = new();

                        using (StreamGeometryContext ctx = geometry.Open())
                        {
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
                        }

                        double? fontSize = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.FontSize;
                        if (fontSize == null)
                        {
                            return;
                        }

                        float strokeThickness = (float)Math.Max(fontSize.Value / 15, 1);

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
        SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.TextArea.TextView.Redraw(segment);
    }

    public event EventHandler? RedrawRequested;

    protected override void ColorizeLine(DocumentLine line)
    {
    }

    private static IEnumerable<Point> CreatePoints(Point start, Point end, double offset, int count)
    {
        double? fontSize = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.FontSize;
        if (fontSize == null)
        {
            yield break;
        }

        double multiplier = fontSize.Value * 0.075;

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

public sealed class TextMarker : TextSegment, ITextMarker
{
    private readonly TextMarkerService _service;

    public TextMarker(TextMarkerService service, int startOffset, int length)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        StartOffset = startOffset;
        Length = length;
        MarkerTypes = TextMarkerTypes.None;
    }

    public bool IsDeleted => !IsConnectedToCollection;

    public Color? BackgroundColor { get; set; }
    public Color? ForegroundColor { get; set; }
    public TextMarkerTypes MarkerTypes { get; set; }
    public Color MarkerColor { get; set; }

    public Func<UserControl>? Tooltip { get; set; }

    public void Delete()
    {
        _service.Remove(this);
    }
}