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

    public List<Notification> History { get; } = [];

    public NotificationService(WindowRegistry windowRegistry, IMessenger messenger)
    {
        _windowRegistry = windowRegistry;

        messenger.Register(this);
    }

    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(4);

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
        Show(message.Notification);
    }

    public void Show(Notification notification)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SkEditorNotificationManager? manager = _windowRegistry.Active?.NotificationManager;
            manager?.Show(
                notification.Title.Value,
                notification.Message?.Value ?? string.Empty,
                ToAvaloniaType(notification.Type),
                notification.Duration ?? DefaultDuration);
        });

        History.Add(notification);
    }

    public void Debug(string message)
    {
        Notification notification = Notification
            .Create(TranslatableString.FromRawString("Debug"))
            .WithMessage(TranslatableString.FromRawString(message))
            .WithType(Models.Notifications.NotificationType.Information)
            .WithDuration(TimeSpan.FromSeconds(10));

        Show(notification);
    }
}