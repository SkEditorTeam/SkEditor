using System.Collections.Generic;
using Notification = SkEditor.Models.Notifications.Notification;

namespace SkEditor.Contracts.Services;

public interface INotificationService
{
    void Show(Notification notification);
    void Debug(string message);

    IEnumerable<Notification> GetHistory();
    void ClearHistory();
    void RemoveFromHistory(Notification notification);
}