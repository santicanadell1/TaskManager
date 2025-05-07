using DataAccess;
using DataAccess.ProjectRepositoryExceptions;
using Domain;
using Domain.Exceptions;
using Service;
using Service.Models;

public class AdminPService
{
    private readonly InMemoryDatabase _database;

    public AdminPService(InMemoryDatabase database)
    {
        _database = database;
    }

    public void CreateProject(ProjectDTO projectDTO)
    {
        var existingProject = _database.projects.GetProject(p => p.Name == projectDTO.Name);
        if (existingProject != null)
        {
            throw new DuplicatedProjectsNameException();
        }

        var newProject = new Project(projectDTO.Name, projectDTO.Description, projectDTO.StartDate);
        _database.projects.AddProject(newProject);
    }

    public void AssignMembersToProject(string projectName, List<UserDTO> membersDTO)
    {
        var project = _database.projects.GetProject(p => p.Name == projectName);
        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        foreach (var memberDTO in membersDTO)
        {
            var user = ToEntity(memberDTO);

            if (!project.Members.Contains(user))
            {
                project.AddMember(user);
            }
        }

        _database.projects.UpdateProject(projectName, project);
    }

    private User ToEntity(UserDTO userDTO)
    {
        return new User
        {
            FirstName = userDTO.FirstName,
            LastName = userDTO.LastName,
            Email = userDTO.Email,
            Birthday = userDTO.Birthday,
            Password = userDTO.Password,
            Roles = userDTO.Roles
        };
    }
}