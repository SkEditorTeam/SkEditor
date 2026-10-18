using System;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Notifications;

/// <summary>
///     Represents an in-app toast notification displayed to the user within the SkEditor UI.
/// </summary>
public class Notification
{
    /// <summary> How long the notification remains visible. If null, the default timeout is applied. </summary>
    public TimeSpan? Duration { get; private set; }

    /// <summary> The optional localized description body of the notification. </summary>
    public TranslatableString? Message { get; private set; }

    /// <summary> The localized title displayed in the header of the notification. </summary>
    public TranslatableString Title { get; }

    /// <summary> The severity level which determines the visual style and icon of the notification. </summary>
    public NotificationType Type { get; private set; } = NotificationType.Information;

    private Notification(TranslatableString title) => Title = title;

    /// <summary>
    ///     Creates a new notification with a required title and optional styling parameters.
    /// </summary>
    /// <param name="title">The localized title of the notification. Cannot be null.</param>
    /// <param name="message">An optional detailed description. If null, only the title is displayed in the UI.</param>
    /// <param name="type">The severity level which determines the visual style and icon.</param>
    /// <param name="duration">How long the notification remains visible. If null, the default timeout is applied.</param>
    /// <returns>A configured <see cref="Notification" /> instance ready to be sent to the notification service.</returns>
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

    /// <summary> Sets or updates the detailed message body of the notification and returns this instance for chaining. </summary>
    public Notification WithMessage(TranslatableString message)
    {
        Message = message;
        return this;
    }

    /// <summary> Sets the visual type (severity level) of the notification and returns this instance for chaining. </summary>
    public Notification WithType(NotificationType type)
    {
        Type = type;
        return this;
    }

    /// <summary>
    ///     Sets a custom display duration before the notification automatically disappears and returns this instance for
    ///     chaining.
    /// </summary>
    public Notification WithDuration(TimeSpan duration)
    {
        Duration = duration;
        return this;
    }

    /// <summary> Configures the notification to remain on screen indefinitely and returns this instance for chaining. </summary>
    public Notification MakePersistent()
    {
        Duration = TimeSpan.MaxValue;
        return this;
    }
}