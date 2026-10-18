using System;
using R3;
using SkEditor.Configuration.Configs;

namespace SkEditor.Editor.Features.View.Zoom;

public class ZoomService : IZoomService, IDisposable
{
    private DisposableBag _disposableBag;
    
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

    public ZoomService(AppSettings appSettings) =>
        appSettings
            .ObservePropertyChanged(x => x.EditorFontSize)
            .Subscribe(newValue => GlobalFontSize = newValue)
            .AddTo(ref _disposableBag);

    public event Action<double>? ZoomChanged;

    public void Dispose()
    {
        _disposableBag.Dispose();
        GC.SuppressFinalize(this);
    }
}