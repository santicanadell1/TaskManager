using Service.Models;

namespace Service.Interface;

public interface INotificationService
{
    List<NotificationDTO> GetNotificationsForUser(string userEmail);

    void CreateNotification(NotificationDTO notificationDTO);

    void AddNotificationToUser(string userEmail, int? notificationId);

    void RemoveNotificationFromUser(string userEmail, int? notificationId);
}