using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Settings.Models.Types;

namespace SkEditor.Converters;

public class InfoBarSeverityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not InfoSetting.InfoType infoType)
        {
            return null;
        }

        return infoType switch
        {
            InfoSetting.InfoType.Info => FAInfoBarSeverity.Informational,
            InfoSetting.InfoType.Warning => FAInfoBarSeverity.Warning,
            InfoSetting.InfoType.Error => FAInfoBarSeverity.Error,
            _ => FAInfoBarSeverity.Informational
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}