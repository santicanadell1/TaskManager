using DataAccess;
using DataAccess.Exceptions.NotificationRepositoryExceptions;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service.Models;
using Task = Domain.Task;

namespace Service;

public class NotificationService
{
    private readonly IRepositoryManager _repositoryManager;

    public NotificationService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    private NotificationDTO FromEntity(Notification notification)
    {
        NotificationDTO notificationDTO = new NotificationDTO
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
        List<RolDTO> rolDTOs = new List<RolDTO>();

        if (roles != null)
        {
            foreach (Rol role in roles)
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

    private List<Rol> ConvertToDomainRoles(List<RolDTO> roleDTOs)
    {
        List<Rol> roles = new List<Rol>();

        if (roleDTOs != null)
        {
            foreach (RolDTO roleDTO in roleDTOs)
                switch (roleDTO)
                {
                    case RolDTO.AdminSystem:
                        roles.Add(Rol.AdminSystem);
                        break;
                    case RolDTO.ProjectMember:
                        roles.Add(Rol.ProjectMember);
                        break;
                    case RolDTO.AdminProject:
                        roles.Add(Rol.AdminProject);
                        break;
                }
        }

        return roles;
    }

    private Notification ToEntity(NotificationDTO notificationDTO)
    {
        Project project = _repositoryManager.ProjectRepository.Get(p => p.Name == notificationDTO.Project.Name);

        if (project == null)
        {
            throw new InvalidOperationException($"Project '{notificationDTO.Project.Name}' not found");
        }

        Notification notification = new Notification(
            notificationDTO.Read ?? false,
            notificationDTO.Description,
            project
        );

        return notification;
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
                if (notification != null) notifications.Add(FromEntity(notification));
            }
        }

        return notifications;
    }

    public void CreateNotification(NotificationDTO notificationDTO)
    {
        Notification notification = ToEntity(notificationDTO);
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