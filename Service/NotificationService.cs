using DataAccess;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service.Models;

namespace Service;

public class NotificationService
{
    private readonly ProjectRepository _projectRepository;
    private readonly UserRepository _userRepository;
    private readonly  NotificationRepository _notificationRepository;

    public NotificationService(UserRepository userRepository, ProjectRepository projectRepository,NotificationRepository notificationRepository)
    {
        _userRepository = userRepository;
        _projectRepository = projectRepository;
        _notificationRepository = notificationRepository;
    }

    private NotificationDTO FromEntity(Notification notification)
    {
        var projectService = new AdminPService(_userRepository,_projectRepository,_notificationRepository);
        var notificationDTO = new NotificationDTO();
        notificationDTO.Read = notification.Read;
        notificationDTO.Description = notification.Description;
        notificationDTO.Project = projectService.GetProjectByName(notification.Project.Name);
        notificationDTO.Id = notification.Id;
        return notificationDTO;
    }


    private Notification ToEntity(NotificationDTO notificationDTO)
    {
        var project = _projectRepository.GetProject(p => p.Name == notificationDTO.Project.Name);
        var notification = new Notification((bool)notificationDTO.Read, notificationDTO.Description, project);
        notification.Id = notificationDTO.Id;
        return notification;
    }

    public List<NotificationDTO> GetNotificationsForUser(string userEmail)
    {
        var user = _userRepository.Get(u => u.Email == userEmail);
        if (user == null) throw new UserNotFoundException();
        var notifications = new List<NotificationDTO>();
        foreach (var notificationId in user.Notifications)
        {
            var notification = _notificationRepository.Get(n => n.Id == notificationId);
            if (notification != null) notifications.Add(FromEntity(notification));
        }

        return notifications;
    }

    public void CreateNotification(NotificationDTO notificationDTO)
    {
        var notification = ToEntity(notificationDTO);
        _notificationRepository.Add(notification);
        var noti = _notificationRepository.Get(n => n.description == notification.description);
        foreach (var user in _projectRepository.GetProject(p => p.Name == notification.Project.Name).Members)
            AddNotificationToUser(user.Email, noti.Id);
    }

    public void AddNotificationToUser(string userEmail, int notificationId)
    {
        var user = _userRepository.Get(u => u.Email == userEmail);
        if (user == null) throw new UserNotFoundException();
        user.Notifications.Add(notificationId);
        _userRepository.Update(user.Email, user);
    }

    public void RemoveNotificationFromUser(string userEmail, int notificationId)
    {
        var user = _userRepository.Get(u => u.Email == userEmail);
        if (user == null) throw new UserNotFoundException();
        user.Notifications.Remove(notificationId);
    }
}