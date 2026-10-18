using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace SkEditor.Converters.Layout;

public class HorizontalAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool isVertical)
        {
            return null;
        }

        return isVertical
            ? HorizontalAlignment.Stretch
            : HorizontalAlignment.Left;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}