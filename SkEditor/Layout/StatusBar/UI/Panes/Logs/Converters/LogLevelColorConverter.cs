using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using SkEditor.Core.Logging;

namespace SkEditor.Layout.StatusBar.UI.Panes.Logs.Converters;

public class LogLevelColorConverter : IValueConverter
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
            LogLevel.Debug => app.FindResource("LogLevelDebugColor"),
            LogLevel.Info => app.FindResource("LogLevelInfoColor"),
            LogLevel.Warning => app.FindResource("LogLevelWarningColor"),
            LogLevel.Error => app.FindResource("LogLevelErrorColor"),
            LogLevel.Fatal => app.FindResource("LogLevelFatalColor"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}