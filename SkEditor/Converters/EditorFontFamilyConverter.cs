using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SkEditor.Converters;

public class EditorFontFamilyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        FontFamily? font;

        if (value is not null)
        {
            try
            {
                font = FontFamily.Parse(value.ToString()!);
            }
            catch (Exception)
            {
                font = GetJetBrainsFont();
            }
        }
        else
        {
            font = GetJetBrainsFont();
        }

        return font;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static FontFamily? GetJetBrainsFont()
    {
        return Application.Current?.FindResource("JetBrainsFont") as FontFamily;
    }
}