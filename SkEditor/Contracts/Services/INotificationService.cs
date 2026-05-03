using System;
using Avalonia.Controls.Notifications;
using SkEditor.Utilities.Localization;

namespace SkEditor.Contracts.Services;

public interface INotificationService
{
    void Show(TranslatableString title, TranslatableString message,
        NotificationType type = NotificationType.Information,
        TimeSpan? duration = null);

    [Obsolete("Use the overload that accepts TranslatableString instead.")]
    void Show(string title, string message,
        NotificationType type = NotificationType.Information,
        TimeSpan? duration = null);

    void ShowSuccess(TranslatableString title, TranslatableString message);
    void ShowError(TranslatableString title, TranslatableString message);
    void ShowWarning(TranslatableString title, TranslatableString message);

    void Debug(string message);
}