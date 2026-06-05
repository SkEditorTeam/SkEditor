using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SkEditor.Services.Localization;
using SkEditor.Utilities.Localization;

namespace SkEditor.Converters.Documentation;

public class DocumentationNullableAddonNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not null)
        {
            return value;
        }

        return TranslationService.Current.GetObservable(TranslationKey.DocumentationAllAddons);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}