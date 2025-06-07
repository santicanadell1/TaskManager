using DataAccess;
using DataAccess.Exceptions.NotificationRepositoryExceptions;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service.Converter;
using Service.Converters;
using Service.Models;
using Task = Domain.Task;

namespace Service
{
    public class NotificationService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly NotificationConverter _notificationConverter;
        private readonly ResourceConverter _resourceConverter;
        private readonly RolConverter _rolConverter;
        private readonly TaskConverter _taskConverter;
        private readonly UserConverter _userConverter;
        private readonly ProjectConverter _projectConverter;

        public NotificationService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
            _rolConverter = new RolConverter();
            _resourceConverter = new ResourceConverter(_repositoryManager);
            _taskConverter = new TaskConverter(_repositoryManager, _resourceConverter);
            _userConverter = new UserConverter(_repositoryManager, _rolConverter, _taskConverter);
            _projectConverter = new ProjectConverter(_repositoryManager, _userConverter);
            _notificationConverter = new NotificationConverter(repositoryManager, _projectConverter);
        }

        public List<NotificationDTO> GetNotificationsForUser(string userEmail)
        {
            User user = _repositoryManager.UserRepository.Get(u => u.Email == userEmail);
            if (user == null) throw new UserNotFoundException();

            List<NotificationDTO> notifications = new List<NotificationDTO>();
            if (user.Notifications != null)
            {
                foreach (Notification notification in user.Notifications)
                {
                    if (notification != null)
                        notifications.Add(_notificationConverter.FromEntity(notification));
                }
            }

            return notifications;
        }

        public void CreateNotification(NotificationDTO notificationDTO)
        {
            Notification notification = _notificationConverter.ToEntity(notificationDTO);
            _repositoryManager.NotificationRepository.Add(notification);

            Notification createdNotification = _repositoryManager.NotificationRepository.Get(n =>
                n.Description == notification.Description &&
                n.Project.Name == notification.Project.Name);

            if (createdNotification == null)
            {
                throw new InvalidOperationException("Failed to create notification");
            }

            Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == notification.Project.Name);

            if (project?.Members != null)
            {
                foreach (User user in project.Members)
                {
                    AddNotificationToUser(user.Email, createdNotification.Id);
                }
            }
        }

        public void AddNotificationToUser(string userEmail, int? notificationId)
        {
            User user = _repositoryManager.UserRepository.Get(u => u.Email == userEmail);
            if (user == null) throw new UserNotFoundException();

            Notification notificationToAdd = _repositoryManager.NotificationRepository.Get(n => n.Id == notificationId);
            if (notificationToAdd != null)
            {
                bool alreadyExists = user.Notifications.Any(n => n.Id == notificationToAdd.Id);
                if (!alreadyExists)
                {
                    user.Notifications.Add(notificationToAdd);
                    _repositoryManager.UserRepository.Update(user);
                }
            }
        }

        public void RemoveNotificationFromUser(string userEmail, int? notificationId)
        {
            User user = _repositoryManager.UserRepository.Get(u => u.Email == userEmail);
            if (user == null) throw new UserNotFoundException();

            if (user.Notifications != null)
            {
                Notification includeNotification = user.Notifications.Find(n => n.Id == notificationId);
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
}