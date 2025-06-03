using DataAccess;
using DataAccess.Exceptions.NotificationRepositoryExceptions;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service.Models;
using Task = Domain.Task;

namespace Service;

public class NotificationService
{
    private readonly ProjectRepository _projectRepository;
    private readonly UserRepository _userRepository;
    private readonly NotificationRepository _notificationRepository;

    public NotificationService(UserRepository userRepository, ProjectRepository projectRepository,
        NotificationRepository notificationRepository)
    {
        _userRepository = userRepository;
        _projectRepository = projectRepository;
        _notificationRepository = notificationRepository;
    }

    private NotificationDTO FromEntity(Notification notification)
    {
        var notificationDTO = new NotificationDTO
        {
            Id = notification.Id,
            Read = notification.Read,
            Description = notification.Description,
            Project = FromEntitySimple(notification.Project)
        };

        return notificationDTO;
    }

    private ProjectDTO FromEntitySimple(Project project)
    {
        return new ProjectDTO
        {
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            Members = project.Members?.Select(member => FromEntitySimple(member)).ToList() ?? new List<UserDTO>(),
            AdminProyect = project.AdminProject != null ? FromEntitySimple(project.AdminProject) : null
        };
    }

    private UserDTO FromEntitySimple(User user)
    {
        return new UserDTO
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = ConvertToDTORoles(user.Roles),
            Password = user.Password,
            Birthday = user.Birthday
        };
    }

    private List<RolDTO> ConvertToDTORoles(List<Rol> roles)
    {
        var rolDTOs = new List<RolDTO>();

        if (roles != null)
        {
            foreach (var role in roles)
                switch (role)
                {
                    case Rol.AdminSystem:
                        rolDTOs.Add(RolDTO.AdminSystem);
                        break;
                    case Rol.ProjectMember:
                        rolDTOs.Add(RolDTO.ProjectMember);
                        break;
                    case Rol.AdminProject:
                        rolDTOs.Add(RolDTO.AdminProject);
                        break;
                }
        }

        return rolDTOs;
    }
    
    private Notification ToEntity(NotificationDTO notificationDTO)
    {
        var project = _projectRepository.Get(p => p.Name == notificationDTO.Project.Name);
        var notification = new Notification((bool)notificationDTO.Read, notificationDTO.Description, project);
        return notification;
    }

    public List<NotificationDTO> GetNotificationsForUser(string userEmail)
    {
        var user = _userRepository.Get(u => u.Email == userEmail);
        if (user == null) throw new UserNotFoundException();

        var notifications = new List<NotificationDTO>();
        if (user.Notifications != null)
        {
            foreach (var notification in user.Notifications)
            {
                if (notification != null) notifications.Add(FromEntity(notification));
            }
        }

        return notifications;
    }

    public void CreateNotification(NotificationDTO notificationDTO)
    {
        var notification = ToEntity(notificationDTO);
        _notificationRepository.Add(notification);

        var createdNotification = _notificationRepository.Get(n =>
            n.Description == notification.Description &&
            n.Project.Id == notification.Project.Id);

        if (createdNotification == null)
        {
            throw new InvalidOperationException("Failed to create notification");
        }

        var project = _projectRepository.Get(p => p.Name == notification.Project.Name);

        foreach (var user in project.Members)
        {
            AddNotificationToUser(user.Email, createdNotification.Id);
        }
    }

    public void AddNotificationToUser(string userEmail, int? notificationId)
    {
        var user = _userRepository.Get(u => u.Email == userEmail);
        if (user == null) throw new UserNotFoundException();

        if (user.Notifications == null)
        {
            user.Notifications = new List<Notification>();
        }

        var notificationToAdd = _notificationRepository.Get(n => n.Id == notificationId);
        if (notificationToAdd != null)
        {
            bool alreadyExists = user.Notifications.Any(n => n.Id == notificationId);
            if (!alreadyExists)
            {
                user.Notifications.Add(notificationToAdd);
                _userRepository.Update(user);
            }
        }
    }

    public void RemoveNotificationFromUser(string userEmail, int? notificationId)
    {
        var user = _userRepository.Get(u => u.Email == userEmail);
        if (user == null) throw new UserNotFoundException();

        if (user.Notifications != null)
        {
            Notification includeNotification = user.Notifications.Find(n => n.Id == notificationId);
            if (includeNotification != null)
            {
                user.Notifications.Remove(includeNotification);
                _userRepository.Update(user);
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