using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace SkEditor.Converters;

public class ScrollVisibleToPaddingConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is double extent && values[1] is double viewport)
            return extent > viewport ? new Thickness(0, 0, 16, 0) : new Thickness(0);
        return new Thickness(0);
    }
}