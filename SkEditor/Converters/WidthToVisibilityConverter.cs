using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SkEditor.Converters;

public class WidthToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double width)
        {
            return true;
        }

        double threshold = 450;

        if (parameter is string paramString && double.TryParse(paramString, out double parsedThreshold))
        {
            threshold = parsedThreshold;
        }

        return width >= threshold;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}