using System;

namespace SkEditor.Utilities.Editor.TextMarkers;

[Flags]
public enum TextMarkerTypes
{
    None = 0x0000,
    SquigglyUnderline = 0x001,
    NormalUnderline = 0x002
}