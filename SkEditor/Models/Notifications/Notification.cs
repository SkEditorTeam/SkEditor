using System;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Notifications;

public class Notification
{
    private Notification(TranslatableString title) => Title = title;
    public TranslatableString Title { get; }
    public TranslatableString? Message { get; private set; }
    public NotificationType Type { get; private set; } = NotificationType.Information;
    public TimeSpan? Duration { get; private set; }

    public static Notification Create(TranslatableString title, TranslatableString? message = null,
        NotificationType type = NotificationType.Information, TimeSpan? duration = null)
    {
        return new Notification(title)
        {
            Message = message,
            Type = type,
            Duration = duration
        };
    }

    public Notification WithMessage(TranslatableString message)
    {
        Message = message;
        return this;
    }

    public Notification WithType(NotificationType type)
    {
        Type = type;
        return this;
    }

    public Notification WithDuration(TimeSpan duration)
    {
        Duration = duration;
        return this;
    }
}