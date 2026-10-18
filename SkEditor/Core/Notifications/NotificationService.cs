using System;
using System.Collections.Generic;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Core.Localization;
using SkEditor.Core.Notifications.Converters;
using SkEditor.Core.Notifications.Messages;
using SkEditor.Core.Notifications.UI;
using SkEditor.Windowing;

namespace SkEditor.Core.Notifications;

public class NotificationService : INotificationService, IRecipient<ShowNotificationMessage>
{
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(4);

    private readonly List<Notification> _history = [];
    private readonly WindowRegistry _windowRegistry;

    public NotificationService(WindowRegistry windowRegistry, IMessenger messenger)
    {
        _windowRegistry = windowRegistry;

        messenger.Register(this);
    }

    public void Show(Notification notification)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SkEditorNotificationManager? manager = _windowRegistry.Active?.NotificationManager;
            manager?.Show(
                notification.Title.Value,
                notification.Message?.Value ?? string.Empty,
                NotificationTypeConverter.ToAvaloniaType(notification.Type),
                notification.Duration ?? DefaultDuration);
        });

        _history.Add(notification);
    }

    public void Debug(string message)
    {
        Notification notification = Notification
            .Create(TranslatableString.FromRawString("Debug"))
            .WithMessage(TranslatableString.FromRawString(message))
            .WithType(NotificationType.Information)
            .WithDuration(TimeSpan.FromSeconds(10));

        Show(notification);
    }

    public IEnumerable<Notification> GetHistory()
    {
        return _history;
    }

    public void ClearHistory()
    {
        _history.Clear();
    }

    public void RemoveFromHistory(Notification notification)
    {
        _history.Remove(notification);
    }

    public void Receive(ShowNotificationMessage message)
    {
        Show(message.Notification);
    }
}