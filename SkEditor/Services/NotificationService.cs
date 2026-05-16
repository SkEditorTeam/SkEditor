using System;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Contracts.Services;
using SkEditor.Models.Messages;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Localization;
using SkEditor.Views.Controls.Notifications;
using Notification = SkEditor.Models.Notifications.Notification;

namespace SkEditor.Services;

public class NotificationService : INotificationService, IRecipient<ShowNotificationMessage>
{
    private readonly WindowRegistry _windowRegistry;

    public NotificationService(WindowRegistry windowRegistry, IMessenger messenger)
    {
        _windowRegistry = windowRegistry;

        messenger.Register(this);
    }

    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(4);

    private void Show(TranslatableString title, TranslatableString? message,
        NotificationType type =
            NotificationType.Information,
        TimeSpan? duration = null)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SkEditorNotificationManager? manager = _windowRegistry.Active?.NotificationManager;
            manager?.Show(title.Value, message?.Value ?? string.Empty, type, duration ?? DefaultDuration);
        });
    }

    private static NotificationType ToAvaloniaType(Models.Notifications.NotificationType type)
    {
        return type switch
        {
            Models.Notifications.NotificationType.Information => NotificationType.Information,
            Models.Notifications.NotificationType.Success => NotificationType.Success,
            Models.Notifications.NotificationType.Warning => NotificationType.Warning,
            Models.Notifications.NotificationType.Error => NotificationType.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public void Receive(ShowNotificationMessage message)
    {
        Show(message.Title, message.Message, ToAvaloniaType(message.Type), message.Duration);
    }

    public void Show(Notification notification)
    {
        Show(notification.Title, notification.Message, ToAvaloniaType(notification.Type), notification.Duration);
    }

    public void Debug(string message)
    {
        Show(TranslatableString.FromRawString("Debug"), TranslatableString.FromRawString(message),
            NotificationType.Information, TimeSpan.FromSeconds(10));
    }
}