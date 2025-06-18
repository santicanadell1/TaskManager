using Service.Models;

namespace Service;

public class GanttService
{
    public static GanttData Convert(List<TaskDTO> allTasks, List<TaskDTO> criticalPath)
    {
        List<GanttTask> data = new List<GanttTask>();
        List<GanttLink> links = new List<GanttLink>();
        var linkId = 1;

        foreach (var task in allTasks)
        {
            var isInCriticalPath = criticalPath.Any(ct => ct.Id == task.Id);
            var progress = GetProgress(task);
            data.Add(new GanttTask
            {
                id = task.Id ?? 0,
                text = task.Title,
                start_date = task.StartDate == default
                    ? DateTime.Today.ToString("yyyy-MM-dd")
                    : task.StartDate.ToString("yyyy-MM-dd"),
                end_date = task.EndDate == default
                    ? DateTime.Today.ToString("yyyy-MM-dd")
                    : task.EndDate.ToString("yyyy-MM-dd"),
                duration = task.Duration,
                progress = progress,
                critical = isInCriticalPath,
                slack = task.Slack.TotalDays
            });
            foreach (var prev in task.PreviousTasks)
                links.Add(new GanttLink
                {
                    id = linkId++,
                    source = prev.Id ?? 0,
                    target = task.Id ?? 0,
                    type = "0",
                    critical = isInCriticalPath && criticalPath.Any(ct => ct.Id == prev.Id)
                });
        }

        return new GanttData
        {
            data = data,
            links = links,
            criticalPathIds = criticalPath.Select(t => t.Id ?? 0).ToList()
        };
    }

    private static double GetProgress(TaskDTO task)
    {
        double progress = 0;
        switch (task.State)
        {
            case StateDTO.TODO:
                progress = 0.0;
                break;
            case StateDTO.DOING:
                progress = 0.5;
                break;
            case StateDTO.DONE:
                progress = 1.0;
                break;
        }

        return progress;
    }
}