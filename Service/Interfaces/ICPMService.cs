using Service.Models;

namespace Service.Interface;

public interface ICpmService
{
    CpmResult CalculateCriticalPath(List<TaskDTO> tasks);

    DateTime CalculateEarlyStart(TaskDTO task);

    DateTime CalculateEarlyFinish(TaskDTO task);

    DateTime CalculateLateStart(TaskDTO task, List<TaskDTO> allTasks);

    DateTime CalculateLateFinish(TaskDTO task, List<TaskDTO> allTasks);

    bool IsCritical(TaskDTO task);
}