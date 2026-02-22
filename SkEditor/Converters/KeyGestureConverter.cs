using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Input;

namespace SkEditor.Converters;

public class KeyGestureConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string inputGesture
            ? null
            : KeyGesture.Parse(inputGesture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}