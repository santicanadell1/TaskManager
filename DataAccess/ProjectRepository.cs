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
}