using System;
using System.ComponentModel;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.Services.Files.Features;

public class ZoomService : IZoomService
{
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

    public ZoomService(AppSettings appSettings) => appSettings.PropertyChanged += OnPropertyChanged;

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not AppSettings settings)
        {
            return;
        }

        if (e.PropertyName != nameof(AppSettings.EditorFontSize))
        {
            return;
        }

        GlobalFontSize = settings.EditorFontSize;
    }

    public event Action<double>? ZoomChanged;
}