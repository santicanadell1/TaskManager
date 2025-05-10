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

    private void CheckAdminProyectRole()
    {
        var currentUser = LoggedUser.Current;
        if (currentUser == null || !currentUser.Roles.Contains(Rol.AdminProject))
        {
            throw new UnauthorizedAdminAccessException();
        }
    }

    public void CreateProject(ProjectDTO projectDTO)
    {
        CheckAdminProyectRole();
        var existingProject = _database.projects.GetProject(p => p.Name == projectDTO.Name);
        if (existingProject != null)
        {
            throw new DuplicatedProjectsNameException();
        }

        var adminProyect = LoggedUser.Current;

        var newProject = new Project(projectDTO.Name, projectDTO.Description, projectDTO.StartDate);
        newProject.AdminProject = ToEntity(adminProyect);

        _database.projects.AddProject(newProject);
    }

    public void AssignMembersToProject(string projectName, List<UserDTO> membersDTO)
    {
        CheckAdminProyectRole();
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

    public void RemoveProject(string projectName)
    {
        CheckAdminProyectRole();
        _database.projects.RemoveProject(projectName);
    }

    public void UpdateProject(string projectNameToUpdate, ProjectDTO updatedProjectDTO)
    {
        CheckAdminProyectRole();
        _database.projects.UpdateProject(projectNameToUpdate, ToEntity(updatedProjectDTO));
    }

    public List<ProjectDTO> GetAllProjects()
    {
        CheckAdminProyectRole();
        List<Project> projects = _database.projects.GetAllProjects();
        List<ProjectDTO> projectDTOs = new List<ProjectDTO>();
        foreach (var project in projects)
        {
            projectDTOs.Add(FromEntity(project));
        }

        return projectDTOs;
    }

    public ProjectDTO GetProjectByName(string projectName)
    {
        CheckAdminProyectRole();
        var project = _database.projects.GetProject(p => p.Name == projectName);
        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        return FromEntity(project);
    }

    private ProjectDTO FromEntity(Project project)
    {
        return new ProjectDTO()
        {
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
        };
    }

    public Project ToEntity(ProjectDTO projectDTO)
    {
        return new Project
        {
            Name = projectDTO.Name,
            Description = projectDTO.Description,
            StartDate = projectDTO.StartDate
        };
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