using System;
using System.Collections.Generic;
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
    Func<UserControl> Tooltip { get; set; }
    void Delete();
}

[Flags]
public enum TextMarkerTypes
{
    None = 0x0000,
    SquigglyUnderline = 0x001,
    NormalUnderline = 0x002
}

public interface ITextMarkerService
{
    IEnumerable<ITextMarker> TextMarkers { get; }
    ITextMarker Create(int startOffset, int length);

    void Remove(ITextMarker marker);

    void RemoveAll();

    IEnumerable<ITextMarker> GetMarkersAtOffset(int offset);
}