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
        Notification notification = new Notification((bool)notificationDTO.Read, notificationDTO.Description);
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

    public void MarkNotificationAsRead(int idNotification, string userEmail)
    {
        List<Project> projects = _database.projects.GetAllProjects();
        bool notificationFound = false;

        foreach (var project in projects)
        {
            bool isUserMember = project.Members.Any(m => m.Email == userEmail);

            if (!isUserMember) continue;

            var notificationToMark = project.Notifications.FirstOrDefault(n => n.Id == idNotification);

            if (notificationToMark != null)
            {
                notificationToMark.MarkRead();

                _database.notifications.Delete(notificationToMark);

                project.Notifications.Remove(notificationToMark);

                _database.projects.UpdateProject(project.Name, project);

                notificationFound = true;
                break; // only one notification has this id
            }
        }

        if (!notificationFound)
        {
            throw new NotificationNotFoundException();
        }
    }
}