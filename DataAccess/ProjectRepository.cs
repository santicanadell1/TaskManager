using Domain;

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
        Projects.Add(project);
    }

    public List<Project> GetAllProjects()
    {
        return Projects;
    }

    public Project? GetProject(Func<Project, bool> filter)
    {
        return Projects.FirstOrDefault(filter);
    }
}