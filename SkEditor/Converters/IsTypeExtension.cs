using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace SkEditor.Converters;

public class IsTypeExtension : MarkupExtension, IValueConverter
{
    public Type? TargetType { get; set; }

    public IsTypeExtension()
    {
    }

    public IsTypeExtension(Type targetType) => TargetType = targetType;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null && TargetType?.IsInstanceOfType(value) == true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}