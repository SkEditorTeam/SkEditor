using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AvaloniaNotificationType = Avalonia.Controls.Notifications.NotificationType;

namespace SkEditor.Core.Notifications.Converters;

public class NotificationTypeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NotificationType type)
        {
            return ToAvaloniaType(type);
        }

        return AvaloniaNotificationType.Information;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public static AvaloniaNotificationType ToAvaloniaType(NotificationType type)
    {
        return type switch
        {
            NotificationType.Information => AvaloniaNotificationType.Information,
            NotificationType.Success => AvaloniaNotificationType.Success,
            NotificationType.Warning => AvaloniaNotificationType.Warning,
            NotificationType.Error => AvaloniaNotificationType.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}