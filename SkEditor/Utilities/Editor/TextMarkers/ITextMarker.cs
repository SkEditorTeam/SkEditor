using System;
using Avalonia.Controls;
using Avalonia.Media;

namespace SkEditor.Utilities.Editor.TextMarkers;

public interface ITextMarker
{
    int StartOffset { get; }
    int EndOffset { get; }
    int Length { get; }
    Color? BackgroundColor { get; set; }
    Color? ForegroundColor { get; set; }
    TextMarkerTypes MarkerTypes { get; set; }
    Color MarkerColor { get; set; }
    Func<UserControl>? Tooltip { get; set; }
    void Delete();
}