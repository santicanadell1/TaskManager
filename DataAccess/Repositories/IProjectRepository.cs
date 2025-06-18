using Domain;
using Task = Domain.Task;

namespace DataAccess;

public interface IProjectRepository
{
    public List<Project> GetAll();
    public Project? Get(Func<Project, bool> filter);
    public void Add(Project project);
    public void Update(Project entity);
    public void Delete(Project entity);
    public void AddTask(string projectName, Task task);
    public void UpdateTask(string projectName, int? taskId, Task updatedTask);
    public void RemoveTask(string projectName, int? taskId);
    public void AddPreviousTask(string projectName, int? taskId, Task previousTask);
    public void AddResourceToTask(string projectName, int? taskId, Resource resource);
}