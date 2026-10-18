using System;
using Avalonia.Controls;
using Avalonia.Media;

namespace SkEditor.Editor.Markers;

public interface ITextMarker
{
    Color? BackgroundColor { get; set; }
    int EndOffset { get; }
    Color? ForegroundColor { get; set; }
    int Length { get; }
    Color MarkerColor { get; set; }
    TextMarkerTypes MarkerTypes { get; set; }
    int StartOffset { get; }
    Func<UserControl>? Tooltip { get; set; }
    void Delete();
}