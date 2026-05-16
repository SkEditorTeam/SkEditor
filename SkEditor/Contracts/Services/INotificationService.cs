using Notification = SkEditor.Models.Notifications.Notification;

namespace SkEditor.Contracts.Services;

public interface INotificationService
{
    void Show(Notification notification);
    void Debug(string message);
}