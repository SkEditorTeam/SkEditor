using System;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Document;

namespace SkEditor.Utilities.Editor.TextMarkers;

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