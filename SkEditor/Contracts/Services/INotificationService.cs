using System;
using Avalonia.Controls.Notifications;

namespace SkEditor.Contracts.Services;

public interface INotificationService
{
    void Show(string title, string message,
        NotificationType type = NotificationType.Information,
        TimeSpan? duration = null);

    void ShowSuccess(string title, string message);
    void ShowError(string title, string message);
    void ShowWarning(string title, string message);

    void Debug(string message);
}