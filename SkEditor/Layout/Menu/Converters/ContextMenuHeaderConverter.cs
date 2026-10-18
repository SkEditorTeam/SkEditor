using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SkEditor.Converters;

public class ContextMenuHeaderConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        object? header = values[0];
        bool isSeparator = values[1] is true;

        if (isSeparator)
        {
            return "-";
        }

        if (header is string headerString)
        {
            return headerString;
        }

        return null;
    }
}