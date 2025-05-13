using DataAccess.ProjectRepositoryExceptions;
using Domain.Exceptions;
using DataAccess.Exceptions.UserRepositoryExceptions;
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

    private  NotificationDTO FromEntity(Notification notification)
    {
        AdminPService projectService = new AdminPService(_database);
        NotificationDTO notificationDTO = new NotificationDTO();
        notificationDTO.Read = notification.Read;
        notificationDTO.Description = notification.Description;
        notificationDTO.Project = projectService.GetProjectByName(notification.Project.Name);
        notificationDTO.Id = notification.Id;
        return notificationDTO;
    }


    private  Notification ToEntity(NotificationDTO notificationDTO)
    {
        Project project = _database.projects.GetProject(p=> p.Name == notificationDTO.Project.Name);
        Notification notification = new Notification((bool)notificationDTO.Read, notificationDTO.Description, project);
        notification.Id = notificationDTO.Id;
        return notification;
    }

    public List<NotificationDTO> GetNotificationsForUser(string userEmail)
    {
        User user = _database.users.Get(u => u.Email == userEmail);
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        List<NotificationDTO> notifications = new List<NotificationDTO>();
        foreach (var notificationId in user.Notifications)
        {
            var notification = _database.notifications.Get(n => n.Id == notificationId);
            if (notification != null)
            {
                notifications.Add(FromEntity(notification));
            }
        }

        return notifications;
    }
    public void CreateNotification(NotificationDTO notificationDTO)
    {
        Notification notification = ToEntity(notificationDTO);
        _database.notifications.AddNotification(notification);
        Notification noti = _database.notifications.Get(n => n.description == notification.description);
        foreach (var user in _database.projects.GetProject(p => p.Name == notification.Project.Name).Members)
        {
            AddNotificationToUser(user.Email, noti.Id);
        }
    }

    public void AddNotificationToUser(string userEmail, int notificationId)
    {
        User user = _database.users.Get(u => u.Email == userEmail);
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        user.Notifications.Add(notificationId);
        _database.users.Update(user.Email, user);
    }
    
    public void RemoveNotificationFromUser(string userEmail, int notificationId)
    {
        User user = _database.users.Get(u => u.Email == userEmail);
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        user.Notifications.Remove(notificationId);
    }
}