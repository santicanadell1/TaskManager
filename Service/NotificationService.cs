using Service.Models;

namespace Service;
using Domain;
using DataAccess;
public class NotificationService
{
    private readonly InMemoryDatabase _database;
    
    public NotificationService(InMemoryDatabase database)
    {
        _database = database;
    }

    public static NotificationDTO FromEntity(Notification notification)
    {
        NotificationDTO notificationDTO = new NotificationDTO();
        notificationDTO.Read = notification.Read;
        notificationDTO.Description = notification.Description;
        return notificationDTO;
    }
}