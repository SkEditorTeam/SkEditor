using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;

namespace SkEditor.Appearance.Icons.Converters;

public class StandardIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not StandardIcon settingIcon)
        {
            return null;
        }

        if (settingIcon.IsSymbol)
        {
            return new SymbolIcon { Symbol = (Symbol)settingIcon.Value };
        }

        return settingIcon.IsPath
            ? Icons.GetPathIcon((string)settingIcon.Value)
            : null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}