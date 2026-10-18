using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SkEditor.Core.Localization;
using SkEditor.Workspace.Remote.Models;

namespace SkEditor.Workspace.Remote.Converters;

public class AuthMethodConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            RemoteAuthMethod.Password => TranslationService.Current.Get(TranslationKey
                .RemoteConnectionFieldAuthMethodPassword),
            RemoteAuthMethod.PrivateKey => TranslationService.Current.Get(TranslationKey
                .RemoteConnectionFieldAuthMethodKey),
            _ => value?.ToString()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}