using DataAccess.ProjectRepositoryExceptions;
using Domain.Exceptions;
using Domain.Exceptions.UserRepositoryExceptions;
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

    private static NotificationDTO FromEntity(Notification notification)
    {
        NotificationDTO notificationDTO = new NotificationDTO();
        notificationDTO.Read = notification.Read;
        notificationDTO.Description = notification.Description;
        notificationDTO.Id = notification.Id;
        return notificationDTO;
    }

    private static Notification ToEntity(NotificationDTO notificationDTO)
    {
        Notification notification = new Notification((bool)notificationDTO.Read, notificationDTO.Description,);
        return notification;
    }

    public List<NotificationDTO> GetNotificationsForUser(string userEmail)
    {
        User user = _database.users.Get(u => u.Email == userEmail);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        return user.Notifications.Select(n => FromEntity(n)).ToList();
    }

    public void AddNotificationToProject(string projectName, NotificationDTO notificationDTO)
    {
        Project project = _database.projects.GetProject(p => p.Name == projectName);
        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        Notification notification = ToEntity(notificationDTO);

        foreach (var member in project.Members)
        {
            if (member.Notifications == null)
            {
                member.Notifications = new List<Notification>();
            }

            var userNotification = new Notification(notification.Read, notification.Description)
            {
                Id = notification.Id
            };

            member.Notifications.Add(userNotification);

            _database.users.Update(member.Email, member);
        }

        project.Notifications.Add(notification);
        _database.projects.UpdateProject(projectName, project);
    }

    public void RemoveNotificationFromProject(string projectName, int idNotification)
    {
        Project project = _database.projects.GetProject(p => p.Name == projectName);
        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        var notificationToRemove = project.Notifications.FirstOrDefault(n => n.Id == idNotification);
        if (notificationToRemove != null)
        {
            project.Notifications.Remove(notificationToRemove);
        }

        _database.projects.UpdateProject(projectName, project);
    }

    public void MarkNotificationAsRead(int idNotification, string userEmail)
    {
        User user = _database.users.Get(u => u.Email == userEmail);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        if (user.Notifications == null || !user.Notifications.Any())
        {
            throw new NotificationNotFoundException();
        }

        var notification = user.Notifications.FirstOrDefault(n => n.Id == idNotification);
        if (notification == null)
        {
            throw new NotificationNotFoundException();
        }

        notification.MarkRead();
        user.Notifications.Remove(notification);

        _database.users.Update(userEmail, user);
    }
}