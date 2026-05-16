using System;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Messages;

public record ShowNotificationMessage(TranslatableString Title, TranslatableString Message,
    NotificationType Type = NotificationType.Information, TimeSpan? Duration = null);