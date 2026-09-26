using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SkEditor.Converters.Settings;

public class SettingsFontFamilyConverter : IValueConverter
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
                return AvaloniaProperty.UnsetValue;
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