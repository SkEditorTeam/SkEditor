using System;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using SkEditor.Contracts.Services;
using SkEditor.Services.Windowing;
using SkEditor.Views.Controls.Notifications;

namespace SkEditor.Services;

public class NotificationService(WindowRegistry windowRegistry) : INotificationService
{
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(4);

    public void Show(string title, string message,
        NotificationType type = NotificationType.Information,
        TimeSpan? duration = null)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SkEditorNotificationManager? manager = windowRegistry.Active?.NotificationManager;
            manager?.Show(title, message, type, duration ?? DefaultDuration);
        });
    }

    public void ShowSuccess(string title, string message)
    {
        Show(title, message, NotificationType.Success);
    }

    public void ShowError(string title, string message)
    {
        Show(title, message, NotificationType.Error, TimeSpan.FromSeconds(8));
    }

    public void ShowWarning(string title, string message)
    {
        Show(title, message, NotificationType.Warning);
    }

    public void Debug(string message)
    {
        Show("Debug", message, NotificationType.Information, TimeSpan.FromSeconds(10));
    }
}