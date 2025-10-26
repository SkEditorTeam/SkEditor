using System;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Layout;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json.Linq;

namespace SkEditor.API.Settings.Types;

/// <summary>
///     Represent a slider setting.
/// </summary>
public class SliderSetting(
    double min = 0.0, 
    double max = 100.0, 
    double tickFrequency = 1.0, 
    bool showAlternativeInput = true, 
    bool isSnapToTickEnabled = true, 
    bool debounce = true
    ) : ISettingType
{
    public double Min { get; } = min;
    public double Max { get; } = max;
    public double TickFrequency { get; } = tickFrequency;
    public bool IsSnapToTickEnabled { get; } = isSnapToTickEnabled;
    public bool Debounce { get; } = debounce;
    public bool ShowAlternativeInput { get; } = showAlternativeInput;

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
        double initial = (double)raw;

        var slider = CreateSlider(initial);
        NumberBox? numberBox = ShowAlternativeInput ? CreateNumberBox(initial) : null;

        if (Debounce)
        {
            SetupDebouncedSlider(slider, numberBox, onChanged);
        }
        else
        {
            SetupImmediateSlider(slider, numberBox, onChanged);
        }

        if (ShowAlternativeInput && numberBox is not null)
        {
            SetupNumberBox(numberBox, slider, onChanged);

            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5
            };

            panel.Children.Add(numberBox);
            panel.Children.Add(slider);
            return panel;
        }

        return slider;
    }

    private Slider CreateSlider(double value)
    {
        return new Slider
        {
            Minimum = Min,
            Maximum = Max,
            Value = value,
            TickFrequency = TickFrequency,
            IsSnapToTickEnabled = IsSnapToTickEnabled,
            Width = 150,
            MinWidth = 50,
        };
    }

    private NumberBox CreateNumberBox(double value)
    {
        return new NumberBox
        {
            Minimum = Min,
            Maximum = Max,
            Value = value,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    private static void SetupImmediateSlider(Slider slider, NumberBox? numberBox, Action<object> onChanged)
    {
        slider.ValueChanged += (_, _) =>
        {
            onChanged(slider.Value);
            if (numberBox is not null)
            {
                numberBox.Value = slider.Value;
            }
        };
    }

    private static void SetupDebouncedSlider(Slider slider, NumberBox? numberBox, Action<object> onChanged)
    {
        var debounceTimer = new Timer(300) { AutoReset = false };
        double latestValue = slider.Value;

        debounceTimer.Elapsed += (_, _) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                onChanged(latestValue);
                if (numberBox is not null)
                {
                    numberBox.Value = slider.Value;
                }
            });
        };

        slider.ValueChanged += (_, _) =>
        {
            latestValue = slider.Value;
            debounceTimer.Stop();
            debounceTimer.Start();
        };
    }

    private static void SetupNumberBox(NumberBox numberBox, Slider slider, Action<object> onChanged)
    {
        numberBox.ValueChanged += (_, _) =>
        {
            var newValue = numberBox.Value;

            if (Double.IsNaN(newValue))
            {
                numberBox.Value = slider.Value;
                return;
            }

            onChanged(newValue);
            slider.Value = newValue;
        };
    }

    public bool IsSelfManaged => false;
}