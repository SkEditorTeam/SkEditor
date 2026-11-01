using System;
using System.Collections.Generic;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Core.Localization;
using SkEditor.Core.Notifications.Messages;
using SkEditor.Core.Notifications.UI;
using SkEditor.Windowing;

namespace SkEditor.Core.Notifications;

public class NotificationService : INotificationService, IRecipient<ShowNotificationMessage>
{
    private readonly List<Notification> _history = [];
    private readonly IWindowRegistry _windowRegistry;

    public NotificationService(IWindowRegistry windowRegistry, IMessenger messenger)
    {
        _windowRegistry = windowRegistry;

        messenger.Register(this);
    }

    public void Show(Notification notification)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SkEditorNotificationManager? manager = _windowRegistry.Active?.NotificationManager;
            if (manager is null)
            {
                _history.Add(notification);
                return;
            }

            manager.Show(notification, onClosed: closedNotification =>
            {
                _history.Add(closedNotification);
            });
        });
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