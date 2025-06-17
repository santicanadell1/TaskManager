using DataAccess;
using Domain;
using Service.Converter;
using Service.Models;

namespace Service.Converters;

public class NotificationConverter : IConverter<Notification, NotificationDTO>
{
    private readonly ProjectConverter _projectConverter;
    private readonly IRepositoryManager _repositoryManager;

    public NotificationConverter(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _projectConverter = new ProjectConverter(repositoryManager);
    }

    public NotificationDTO FromEntity(Notification notification)
    {
        return new NotificationDTO
        {
            Id = notification.Id,
            Read = notification.IsRead,
            Description = notification.Description,
            Project = _projectConverter.FromEntity(notification.Project)
        };
    }

    public Notification ToEntity(NotificationDTO dto)
    {
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == dto.Project.Name);

        if (project == null)
            throw new InvalidOperationException($"Project '{dto.Project.Name}' not found");

        return new Notification(
            dto.Read ?? false,
            dto.Description,
            project
        );
    }
}