using DataAccess;
using DataAccess.Exceptions.NotificationRepositoryExceptions;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service.Converters;
using Service.Interface;
using Service.Models;

namespace Service;

public class NotificationService : INotificationService
{
    private readonly NotificationConverter _notificationConverter;
    private readonly IRepositoryManager _repositoryManager;

    public NotificationService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _notificationConverter = new NotificationConverter(_repositoryManager);
    }

    public List<NotificationDTO> GetNotificationsForUser(string userEmail)
    {
        var user = _repositoryManager.UserRepository.Get(u => u.Email == userEmail);
        if (user == null) throw new UserNotFoundException();

        var notifications = new List<NotificationDTO>();
        if (user.Notifications != null)
            foreach (var notification in user.Notifications)
                if (notification != null)
                    notifications.Add(_notificationConverter.FromEntity(notification));

        return notifications;
    }

    public void CreateNotification(NotificationDTO notificationDTO)
    {
        var notification = _notificationConverter.ToEntity(notificationDTO);
        _repositoryManager.NotificationRepository.Add(notification);

        var createdNotification = _repositoryManager.NotificationRepository.Get(n =>
            n.Description == notification.Description &&
            n.Project.Name == notification.Project.Name);

        if (createdNotification == null) throw new InvalidOperationException("Failed to create notification");

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == notification.Project.Name);

        if (project?.Members != null)
            foreach (var user in project.Members)
                AddNotificationToUser(user.Email, createdNotification.Id);

        if (project?.AdminProject != null) AddNotificationToUser(project.AdminProject.Email, createdNotification.Id);

        if (project?.ProjectLeader != null) AddNotificationToUser(project.ProjectLeader.Email, createdNotification.Id);
    }

    public void AddNotificationToUser(string userEmail, int? notificationId)
    {
        var user = _repositoryManager.UserRepository.Get(u => u.Email == userEmail);
        if (user == null) throw new UserNotFoundException();

        var notificationToAdd = _repositoryManager.NotificationRepository.Get(n => n.Id == notificationId);
        if (notificationToAdd != null)
        {
            var alreadyExists = user.Notifications.Any(n => n.Id == notificationToAdd.Id);
            if (!alreadyExists)
            {
                user.Notifications.Add(notificationToAdd);
                _repositoryManager.UserRepository.Update(user);
            }
        }
    }

    public void RemoveNotificationFromUser(string userEmail, int? notificationId)
    {
        var user = _repositoryManager.UserRepository.Get(u => u.Email == userEmail);
        if (user == null) throw new UserNotFoundException();

        if (user.Notifications != null)
        {
            var includeNotification = user.Notifications.Find(n => n.Id == notificationId);
            if (includeNotification != null)
            {
                user.Notifications.Remove(includeNotification);
                _repositoryManager.UserRepository.Update(user);
            }
            else
            {
                throw new NotificationNotFoundException();
            }
        }
        else
        {
            throw new NotificationNotFoundException();
        }
    }
}