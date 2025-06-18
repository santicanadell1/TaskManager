using Service;
using Service.Interface;
using Service.Models;

namespace Controllers;

public class GanttController
{
    private readonly ICpmService _cpmService;

    public GanttController()
    {
        _cpmService = new CpmService();
    }

    public GanttData GetGanttData(List<TaskDTO> allTasks, List<TaskDTO> criticalPath)
    {
        return GanttService.Convert(allTasks, criticalPath);
    }

    public CpmResult CalculateCriticalPath(List<TaskDTO> tasks)
    {
        return _cpmService.CalculateCriticalPath(tasks);
    }
}