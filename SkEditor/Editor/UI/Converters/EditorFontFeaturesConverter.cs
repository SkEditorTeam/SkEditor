using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SkEditor.Editor.UI.Converters;

public class EditorFontFeaturesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool ligaturesEnabled)
        {
            return null;
        }

        string featureString = !ligaturesEnabled
            ? "-liga, -calt, -clig"
            : string.Empty;

        return FontFeatureCollection.Parse(featureString);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}