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

    public static Notification ToEntity(NotificationDTO notificationDTO)
    {
        Notification notification = new Notification(notificationDTO.Read,notificationDTO.Description);
        return notification;
    }

    public List<NotificationDTO> GetNotificationsForUser(String userEmail)
    {
        List<NotificationDTO> notifications = new List<NotificationDTO>();
        List<Project> projects = _database.projects.GetAllProjects();
        foreach (var project in projects)
        {
            List<User> members = project.Members;
            foreach (var member in members)
            {
                if (member.Email == userEmail)
                {
                    foreach (var notification in project.Notifications)
                    notifications.Add(FromEntity(notification));
                }
            }
        }
        return notifications;
    }
}

