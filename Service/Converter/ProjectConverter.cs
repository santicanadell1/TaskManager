using DataAccess;
using Domain;
using Service.Converter;
using Service.Models;
using Task = Domain.Task;

public class ProjectConverter : IConverter<Project, ProjectDTO>
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly TaskConverter _taskConverter;
    private readonly UserConverter _userConverter;

    public ProjectConverter(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _userConverter = new UserConverter(repositoryManager);
        _taskConverter = new TaskConverter(repositoryManager);
    }

    public Project ToEntity(ProjectDTO projectDTO)
    {
        var project = new Project
        {
            Id = projectDTO.Id,
            Name = projectDTO.Name,
            Description = projectDTO.Description,
            StartDate = projectDTO.StartDate,
            Members = new List<User>(),
            AdminProject = projectDTO.AdminProyect != null
                ? _repositoryManager.UserRepository.Get(u => u.Email == projectDTO.AdminProyect.Email)
                : null,
            ProjectLeader = projectDTO.ProjectLeader != null
                ? _repositoryManager.UserRepository.Get(u => u.Email == projectDTO.ProjectLeader.Email)
                : null,
            Tasks = new List<Task>()
        };

        if (projectDTO.Tasks != null)
            foreach (var taskDTO in projectDTO.Tasks)
            {
                var taskFromDb = _repositoryManager.TaskRepository.Get(t => t.Id == taskDTO.Id);
                if (taskFromDb != null) project.Tasks.Add(taskFromDb);
            }

        if (projectDTO.Members != null)
            foreach (var memberDTO in projectDTO.Members)
            {
                var memberFromDb = _repositoryManager.UserRepository.Get(u => u.Email == memberDTO.Email);
                if (memberFromDb != null) project.Members.Add(memberFromDb);
            }

        return project;
    }

    public ProjectDTO FromEntity(Project project)
    {
        var memberDTOs = new List<UserDTO>();
        if (project.Members != null)
            foreach (var member in project.Members)
                memberDTOs.Add(_userConverter.FromEntity(member));

        List<TaskDTO> taskDTOs = new List<TaskDTO>();
        if (project.Tasks != null)
            foreach (var task in project.Tasks)
                taskDTOs.Add(_taskConverter.FromEntity(task));

        return new ProjectDTO
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            Members = memberDTOs,
            AdminProyect = project.AdminProject != null ? _userConverter.FromEntity(project.AdminProject) : null,
            ProjectLeader = project.ProjectLeader != null ? _userConverter.FromEntity(project.ProjectLeader) : null,
            Tasks = taskDTOs
        };
    }
}