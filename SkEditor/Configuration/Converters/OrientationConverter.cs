using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace SkEditor.Configuration.Converters;

public class OrientationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool isVertical)
        {
            return null;
        }

        return isVertical ? Orientation.Vertical : Orientation.Horizontal;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}