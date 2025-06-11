using DataAccess;
using Domain;
using Service.Converter;
using Service.Models;

public class ProjectConverter : IConverter<Project, ProjectDTO>
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly UserConverter _userConverter;

    public ProjectConverter(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _userConverter = new UserConverter(repositoryManager);
    }

    public Project ToEntity(ProjectDTO projectDTO)
    {
        Project project = new Project
        {
            Id = projectDTO.Id,
            Name = projectDTO.Name,
            Description = projectDTO.Description,
            StartDate = projectDTO.StartDate,
            Members = new List<User>()
        };

        if (projectDTO.Members != null)
        {
            foreach (UserDTO memberDTO in projectDTO.Members)
            {
                User memberFromDb = _repositoryManager.UserRepository.Get(u => u.Email == memberDTO.Email);
                if (memberFromDb != null)
                {
                    project.Members.Add(memberFromDb);
                }
            }
        }

        return project;
    }

    public ProjectDTO FromEntity(Project project)
    {
        List<UserDTO> memberDTOs = new List<UserDTO>();
        if (project.Members != null)
        {
            foreach (User member in project.Members)
                memberDTOs.Add(_userConverter.FromEntity(member)); 
        }

        return new ProjectDTO
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            Members = memberDTOs,
            AdminProyect = project.AdminProject != null ? _userConverter.FromEntity(project.AdminProject) : null,
            ProjectLeader = project.ProjectLeader != null ? _userConverter.FromEntity(project.ProjectLeader) : null
        };
    }
}