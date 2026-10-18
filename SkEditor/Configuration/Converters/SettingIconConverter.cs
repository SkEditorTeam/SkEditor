using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using SkEditor.Converters.Icons;
using SkEditor.Models.UI;

namespace SkEditor.Converters.Settings;

public class SettingIconConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0 && values[0] is bool isSuccess)
        {
            Symbol symbol = isSuccess
                ? Symbol.CheckmarkCircle
                : Symbol.DismissCircle;

            Color color = isSuccess
                ? Color.Parse("#8BF07B")
                : Color.Parse("#FF2D52");

            return new SymbolIconSource
            {
                Symbol = symbol,
                Foreground = new SolidColorBrush(color)
            };
        }

        if (values.Count > 1 && values[1] is StandardIcon icon)
        {
            return StandardIconSourceConverter.ConvertIcon(icon);
        }

        return null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}