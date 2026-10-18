using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using SkEditor.Core.Logging;

namespace SkEditor.Layout.StatusBar.UI.Panes.Logs.Converters;

public class LogLevelSymbolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LogLevel logLevel)
        {
            return null;
        }

        if (Application.Current is not { } app)
        {
            return null;
        }

        return logLevel switch
        {
            LogLevel.Debug => "DBG",
            LogLevel.Info => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            LogLevel.Fatal => "FTL",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}