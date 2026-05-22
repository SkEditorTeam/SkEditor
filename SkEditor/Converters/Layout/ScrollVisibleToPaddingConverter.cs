using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace SkEditor.Converters.Layout;

public class ScrollVisibleToPaddingConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Vector { Y: > 0 }
            ? new Thickness(0, 0, 16, 0)
            : new Thickness(0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}