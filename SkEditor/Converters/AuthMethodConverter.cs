using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Windows;

namespace SkEditor.Converters;

public class AuthMethodConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            AuthMethod.Password => Translation.Get(TranslationKey.RemoteConnectionFieldAuthMethodPassword),
            AuthMethod.PrivateKey => Translation.Get(TranslationKey.RemoteConnectionFieldAuthMethodKey),
            _ => value?.ToString()
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}