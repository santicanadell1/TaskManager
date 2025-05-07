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
}