using Domain;
using Task = Domain.Task;

namespace DataAccess;

public class ProjectRepository
{
    private List<Project> _projects;
    public ProjectRepository()
    {
        Projects = new List<Project>();
    }
    public List<Project> Projects { get; set; }
    
    public void AddProject(Project project)
    {
        if (Projects.Any(p => p.Name == project.Name))
        {
            throw new ProjectRepositoryExceptions.DuplicatedProjectsNameException();
        }
        else
        {
            Projects.Add(project);
        }
        
    }

    public List<Project> GetAllProjects()
    {
        return Projects;
    }

    public Project? GetProject(Func<Project, bool> filter)
    {
        return Projects.FirstOrDefault(filter);
    }
    
    public void RemoveProject(string name)
    {
        if (!Projects.Any(p => p.Name == name))
        {
            throw new ProjectRepositoryExceptions.ProjectNotFoundException();
        }
        else
        {
            Projects.Remove(Projects.Find(p => p.Name == name));
        }
    }

    public void UpdateProject(string name, Project project)
    {
        if (Projects.Any(p => p.Name == project.Name) && project.Name != name)
        {
            throw new ProjectRepositoryExceptions.DuplicatedProjectsNameException();
        }

        if (!Projects.Any(p => p.Name == name))
        {
            throw new ProjectRepositoryExceptions.ProjectNotFoundException();
        }
        int index = Projects.FindIndex(p => p.Name == name);
        Projects[index] = project;
    }
    
    

}