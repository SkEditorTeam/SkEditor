using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using SkEditor.Utilities.Localization;

namespace SkEditor.Converters;

public class TranslationConverter : IValueConverter
{
    public static readonly TranslationConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not TranslationKey key
            ? BindingOperations.DoNothing
            : Translation.GetObservable(key);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}