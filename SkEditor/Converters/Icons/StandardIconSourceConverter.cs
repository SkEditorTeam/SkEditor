using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using SkEditor.Models.UI;

namespace SkEditor.Converters.Icons;

public class StandardIconSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not StandardIcon settingIcon)
        {
            return null;
        }

        if (settingIcon.IsSymbol)
        {
            return new SymbolIconSource { Symbol = (Symbol)settingIcon.Value };
        }

        if (settingIcon.IsPath)
        {
            return Utilities.Icons.Icons.GetPathIconSource((string)settingIcon.Value);
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}