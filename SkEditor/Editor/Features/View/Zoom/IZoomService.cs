using System;

namespace SkEditor.Contracts.Services.Files.Features;

public interface IZoomService
{
    double GlobalFontSize { get; set; }
    event Action<double> ZoomChanged;
}