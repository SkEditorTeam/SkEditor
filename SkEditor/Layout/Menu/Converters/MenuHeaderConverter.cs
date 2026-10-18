using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;

namespace SkEditor.Layout.Menu.Converters;

public class MenuHeaderConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        object? header = values[0];
        object? iconSymbol = values[1];
        bool isSeparator = values[2] is true;

        if (isSeparator)
        {
            return "-";
        }

        if (header is string headerString)
        {
            return headerString;
        }

        if (iconSymbol is Symbol symbol)
        {
            return new SymbolIcon { Symbol = symbol, FontSize = 18 };
        }

        return null;
    }
}