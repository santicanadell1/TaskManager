using DataAccess.ProjectRepositoryExceptions;
using Domain.Exceptions;
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
        Notification notification = new Notification((bool)notificationDTO.Read, notificationDTO.Description);
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

    public void AddNotificationToProject(string projectName, NotificationDTO notificationDTO)
    {
        Notification notification = ToEntity(notificationDTO);

        Project project = _database.projects.GetProject(p => p.Name == projectName);
        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        project.AddNotification(notification);

        _database.notifications.AddNotification(notification);
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

    public void MarkNotificationAsRead(string projectName, int idNotification)
    {
        Project project = _database.projects.GetProject(p => p.Name == projectName);
        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        var notificationToMark = project.Notifications.FirstOrDefault(n => n.Id == idNotification);
        if (notificationToMark == null)
        {
            throw new NotificationNotFoundException();
        }

        notificationToMark.MarkRead();

        project.Notifications.Remove(notificationToMark);

        _database.notifications.Delete(notificationToMark);

        _database.projects.UpdateProject(projectName, project);
    }
}