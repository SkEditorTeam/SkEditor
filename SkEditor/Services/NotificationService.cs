using System;
using System.Collections.Generic;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Contracts.Services;
using SkEditor.Converters;
using SkEditor.Models.Messages;
using SkEditor.Models.Notifications;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Localization;
using SkEditor.Views.Controls.Notifications;
using Notification = SkEditor.Models.Notifications.Notification;

namespace SkEditor.Services;

public class NotificationService : INotificationService, IRecipient<ShowNotificationMessage>
{
    private readonly WindowRegistry _windowRegistry;

    private readonly List<Notification> _history = [];

    public NotificationService(WindowRegistry windowRegistry, IMessenger messenger)
    {
        _windowRegistry = windowRegistry;

        messenger.Register(this);
    }

    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(4);

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
}