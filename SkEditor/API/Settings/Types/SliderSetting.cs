using System;
using System.Timers;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;

namespace SkEditor.API.Settings.Types;

/// <summary>
///     Represent a slider setting.
/// </summary>
public class SliderSetting(double min = 0.0, double max = 100.0, double tickFrequency = 1.0, bool isSnapToTickEnabled = true, bool debounce = true) : ISettingType
{
    public double Min { get; } = min;
    public double Max { get; } = max;
    public double TickFrequency { get; } = tickFrequency;
    public bool IsSnapToTickEnabled { get; } = isSnapToTickEnabled;
    public bool Debounce { get; } = debounce;
    
    public object Deserialize(JToken value)
    {
        return value.Value<double>();
    }

    public JToken Serialize(object value)
    {
        return new JValue(value);
    }

    public Control CreateControl(object raw, Action<object> onChanged)
    {
        double value = (double)raw;
        Slider slider = new()
        {
            Minimum = Min, 
            Maximum = Max, 
            Value = value,
            TickFrequency = TickFrequency,
            IsSnapToTickEnabled = IsSnapToTickEnabled,
            Width = 150,
            MinWidth = 50,
        };
        
        if (Debounce)
        {
            Timer? debounceTimer = null;
            slider.ValueChanged += (_, args) =>
            {
                debounceTimer?.Stop();
                debounceTimer = new Timer(300);
                debounceTimer.Elapsed += (_, _) =>
                {
                    debounceTimer.Stop();
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => onChanged(slider.Value));
                };
                debounceTimer.AutoReset = false;
                debounceTimer.Start();
            };
        }
        else
        {
            slider.ValueChanged += (_, _) =>
            {
                onChanged(slider.Value);
            };
        }
        
        return slider;
    }

    public bool IsSelfManaged => false;
}