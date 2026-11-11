using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace SkEditor.Core.Common;

public class TwoWayValueConverter<TIn, TOut>(Func<TIn, TOut> convert, Func<TOut, object?> convertBack) : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is TIn val ? convert(val) : default;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is TOut val ? convertBack(val) : AvaloniaProperty.UnsetValue;
}