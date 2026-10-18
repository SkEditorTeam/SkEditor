using System.Collections.Generic;

namespace SkEditor.Core.Notifications;

public interface INotificationService
{
    void Show(Notification notification);
    void Debug(string message);

    IEnumerable<Notification> GetHistory();
    void ClearHistory();
    void RemoveFromHistory(Notification notification);
}