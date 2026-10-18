using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SkEditor.Converters.Settings;

public class SettingsFontFamilyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string fontName || string.IsNullOrWhiteSpace(fontName))
        {
            return AvaloniaProperty.UnsetValue;
        }

        try
        {
            return FontFamily.Parse(fontName);
        }
        catch (Exception)
        {
            return AvaloniaProperty.UnsetValue;
        }
    }


    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}