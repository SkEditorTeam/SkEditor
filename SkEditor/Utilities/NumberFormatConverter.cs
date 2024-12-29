using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SkEditor.Utilities;
public class NumberFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string downloadsStr && int.TryParse(downloadsStr, out int downloads))
        {
            if (downloads >= 1000000)
                return $"{downloads / 1000000.0:F1}M";
            if (downloads >= 1000)
                return $"{downloads / 1000.0:F1}K";
            return downloads.ToString();
        }

        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}