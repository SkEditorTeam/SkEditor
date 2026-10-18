using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SkEditor.Core.Notifications.Converters;

public class NotificationTypeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NotificationType type)
        {
            return ToAvaloniaType(type);
        }

        return Avalonia.Controls.Notifications.NotificationType.Information;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public static Avalonia.Controls.Notifications.NotificationType ToAvaloniaType(NotificationType type)
    {
        return type switch
        {
            NotificationType.Information => Avalonia.Controls.Notifications.NotificationType.Information,
            NotificationType.Success => Avalonia.Controls.Notifications.NotificationType.Success,
            NotificationType.Warning => Avalonia.Controls.Notifications.NotificationType.Warning,
            NotificationType.Error => Avalonia.Controls.Notifications.NotificationType.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}