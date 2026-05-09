using System;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Contracts.Services;
using SkEditor.Models.Messages;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Localization;
using SkEditor.Views.Controls.Notifications;

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

    public void Show(TranslatableString title, TranslatableString message,
        NotificationType type = NotificationType.Information,
        TimeSpan? duration = null)
    {
        Show(title.Value, message.Value, type, duration);
    }

    public void Show(string title, string message,
        NotificationType type = NotificationType.Information,
        TimeSpan? duration = null)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SkEditorNotificationManager? manager = _windowRegistry.Active?.NotificationManager;
            manager?.Show(title, message, type, duration ?? DefaultDuration);
        });
    }

    public void ShowSuccess(TranslatableString title, TranslatableString message)
    {
        Show(title, message, NotificationType.Success);
    }

    public void ShowError(TranslatableString title, TranslatableString message)
    {
        Show(title, message, NotificationType.Error, TimeSpan.FromSeconds(8));
    }

    public void ShowWarning(TranslatableString title, TranslatableString message)
    {
        Show(title, message, NotificationType.Warning);
    }

    public void Debug(string message)
    {
        Show("Debug", message, NotificationType.Information, TimeSpan.FromSeconds(10));
    }

    public void Receive(ShowNotificationMessage message)
    {
        Show(message.Title, message.Message, message.Type, message.Duration);
    }
}