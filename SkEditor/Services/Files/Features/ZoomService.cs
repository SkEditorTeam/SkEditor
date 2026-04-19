using System;
using SkEditor.Contracts.Services.Files.Features;

namespace SkEditor.Services.Files.Features;

public class ZoomService : IZoomService
{
    public event Action<double>? ZoomChanged;

    public double GlobalFontSize
    {
        get;
        set
        {
            double clampedValue = Math.Clamp(value, 5, 200);
            if (Math.Abs(field - clampedValue) < 0.01)
            {
                return;
            }

            field = clampedValue;
            ZoomChanged?.Invoke(clampedValue);
        }
    } = 14;
}