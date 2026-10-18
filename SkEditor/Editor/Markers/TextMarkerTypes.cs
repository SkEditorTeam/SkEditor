using System;

namespace SkEditor.Editor.Markers;

[Flags]
public enum TextMarkerTypes
{
    None = 0x0000,
    SquigglyUnderline = 0x001,
    NormalUnderline = 0x002
}