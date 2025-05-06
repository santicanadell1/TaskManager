using Domain;
using Task = Domain.Task;
using TaskRepositoryExceptions = Domain.Exceptions.TaskRepositoryExceptions;
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
    
    public void AddTask(string projectName, Task task)
    {
        var project = Projects.FirstOrDefault(p => p.Name == projectName);
        if (project == null)
        {
            throw new ProjectRepositoryExceptions.ProjectNotFoundException();
        }

        if (project.Tasks == null)
        {
            project.Tasks = new List<Task>();
        }

        if (project.Tasks.Any(t => t.Id == task.Id))
        {
            throw new TaskRepositoryExceptions.TaskAlreadyExistsException($"Task with ID {task.Id} already exists in project {projectName}.");
        }

        project.Tasks.Add(task); 
    }

    
    public void UpdateTask(string projectName, int? taskId, Task updatedTask)
    {
        var project = Projects.FirstOrDefault(p => p.Name == projectName);
        if (project == null)
        {
            throw new ProjectRepositoryExceptions.ProjectNotFoundException();
        }

        int index = project.Tasks.FindIndex(t => t.Id == taskId);
        if (index == -1)
        {
            throw new TaskRepositoryExceptions.TaskNotFoundException();
        }

        project.Tasks[index] = updatedTask;  
    }

    public void RemoveTask(string projectName, int? taskId)
    {
        var project = Projects.FirstOrDefault(p => p.Name == projectName);
        if (project == null)
        {
            throw new ProjectRepositoryExceptions.ProjectNotFoundException();
        }

        var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
        {
            throw new TaskRepositoryExceptions.TaskNotFoundException();
        }

        project.Tasks.Remove(task);
    }
    
    public void AddPreviousTask(string projectName, int? taskId, Task previousTask)
    {
        var project = Projects.FirstOrDefault(p => p.Name == projectName);
     

        var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
        {
            throw new TaskRepositoryExceptions.TaskNotFoundException();
        }

       
        if (!project.Tasks.Contains(previousTask))
        {
            throw new TaskRepositoryExceptions.TaskNotFoundException();
        }

        task.AddPreviousTask(previousTask); 
    }






}