using DataAccess.Exceptions.ProjectRepositoryExceptions;
using Domain;
using Microsoft.EntityFrameworkCore;
using Task = Domain.Task;
using TaskRepositoryExceptions = DataAccess.Exceptions.TaskRepositoryExceptions;

namespace DataAccess;

public class ProjectRepository
{
    protected readonly AppDbContext _db;

    public ProjectRepository(AppDbContext db)
    {
        _db = db;
    }

    public void AddProject(Project project)
    {
        if (_db.Set<Project>().Any(p => p.Name == project.Name))
            throw new DuplicatedProjectsNameException();

        _db.Set<Project>().Add(project);
        _db.SaveChanges();
    }

    public List<Project> GetAllProjects()
    {
        return _db.Set<Project>()
            .Include(p => p.Tasks)
            .ThenInclude(t => t.Resources)
            .Include(p => p.Tasks)
            .ThenInclude(t => t.PreviousTasks)
            .ToList();
    }

    public Project? GetProject(Func<Project, bool> filter)
    {
        return _db.Set<Project>()
            .Include(p => p.Tasks)
            .ThenInclude(t => t.Resources)
            .Include(p => p.Tasks)
            .ThenInclude(t => t.PreviousTasks)
            .FirstOrDefault(filter);
    }

    public void RemoveProject(string name)
    {
        var project = _db.Set<Project>()
            .Include(p => p.Tasks)
            .FirstOrDefault(p => p.Name == name);

        if (project == null) throw new ProjectNotFoundException();

        _db.Set<Project>().Remove(project);
        _db.SaveChanges();
    }

    public void UpdateProject(string name, Project project)
    {
        if (_db.Set<Project>().Any(p => p.Name == project.Name) && project.Name != name)
            throw new DuplicatedProjectsNameException();

        var existingProject = _db.Set<Project>().FirstOrDefault(p => p.Name == name);
        if (existingProject == null) throw new ProjectNotFoundException();

        existingProject.Description = project.Description;
        existingProject.Name = project.Name;
        _db.SaveChanges();
    }

    public void AddTask(string projectName, Task task)
    {
        var project = _db.Set<Project>()
            .Include(p => p.Tasks)
            .FirstOrDefault(p => p.Name == projectName);

        if (project == null) throw new ProjectNotFoundException();

        project.Tasks.Add(task);
        _db.SaveChanges();
    }

    public void UpdateTask(string projectName, int? taskId, Task updatedTask)
    {
        var project = _db.Set<Project>()
            .Include(p => p.Tasks)
            .ThenInclude(t => t.Resources)
            .Include(p => p.Tasks)
            .ThenInclude(t => t.PreviousTasks)
            .FirstOrDefault(p => p.Name == projectName);

        if (project == null) throw new ProjectNotFoundException();

        var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null) throw new TaskRepositoryExceptions.TaskNotFoundException();

        task.Title = updatedTask.Title;
        task.Description = updatedTask.Description;
        task.State = updatedTask.State;
        task.Duration = updatedTask.Duration;
        task.ExpectedStartDate = updatedTask.ExpectedStartDate;
        task.PreviousTasks = updatedTask.PreviousTasks;
        task.SameTimeTasks = updatedTask.SameTimeTasks;
        task.Resources = updatedTask.Resources;

        _db.SaveChanges();
    }

    public void RemoveTask(string projectName, int? taskId)
    {
        var project = _db.Set<Project>()
            .Include(p => p.Tasks)
            .FirstOrDefault(p => p.Name == projectName);

        if (project == null) throw new ProjectNotFoundException();

        var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null) throw new TaskRepositoryExceptions.TaskNotFoundException();

        project.Tasks.Remove(task);
        _db.SaveChanges();
    }

    public void AddPreviousTask(string projectName, int? taskId, Task previousTask)
    {
        var project = _db.Set<Project>()
            .Include(p => p.Tasks)
            .ThenInclude(t => t.PreviousTasks)
            .FirstOrDefault(p => p.Name == projectName);

        if (project == null) throw new ProjectNotFoundException();

        var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null) throw new TaskRepositoryExceptions.TaskNotFoundException();

        if (!project.Tasks.Contains(previousTask))
            throw new TaskRepositoryExceptions.TaskNotFoundException();

        task.AddPreviousTask(previousTask);
        _db.SaveChanges();
    }

    public void AddResourceToTask(string projectName, int? taskId, Resource resource)
    {
        var project = _db.Set<Project>()
            .Include(p => p.Tasks)
            .ThenInclude(t => t.Resources)
            .FirstOrDefault(p => p.Name == projectName);

        if (project == null) throw new ProjectNotFoundException();

        var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null) throw new TaskRepositoryExceptions.TaskNotFoundException();

        if (task.Resources == null) task.Resources = new List<Resource>();

        task.Resources.Add(resource);
        _db.SaveChanges();
    }
}