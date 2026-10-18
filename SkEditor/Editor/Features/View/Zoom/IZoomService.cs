using System;

namespace SkEditor.Editor.Features.View.Zoom;

public interface IZoomService
{
    double GlobalFontSize { get; set; }
    event Action<double> ZoomChanged;
}