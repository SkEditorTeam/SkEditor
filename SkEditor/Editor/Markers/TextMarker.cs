using System;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Document;

namespace SkEditor.Editor.Markers;

public sealed class TextMarker : TextSegment, ITextMarker
{
    private readonly TextMarkerService _service;

    public Color? BackgroundColor { get; set; }
    public Color? ForegroundColor { get; set; }

    public bool IsDeleted => !IsConnectedToCollection;
    public Color MarkerColor { get; set; }
    public TextMarkerTypes MarkerTypes { get; set; }

    public Func<UserControl>? Tooltip { get; set; }

    public TextMarker(TextMarkerService service, int startOffset, int length)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        StartOffset = startOffset;
        Length = length;
        MarkerTypes = TextMarkerTypes.None;
    }

    public void Delete()
    {
        _service.Remove(this);
    }
}