using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;

namespace SkEditor.Appearance.Icons.Converters;

public class StandardIconSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is StandardIcon settingIcon
            ? ConvertIcon(settingIcon)
            : null;
    }

    public static object? ConvertIcon(StandardIcon icon)
    {
        if (icon.IsSymbol)
        {
            return new SymbolIconSource { Symbol = (Symbol)icon.Value };
        }

        return icon.IsPath
            ? Icons.GetPathIconSource((string)icon.Value)
            : null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}