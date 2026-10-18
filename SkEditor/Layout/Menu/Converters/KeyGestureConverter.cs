using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SkEditor.Layout.Menu.Converters;

public class KeyGestureConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string inputGesture
            ? null
            : KeyGestureUtilities.ParsePlatform(inputGesture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}