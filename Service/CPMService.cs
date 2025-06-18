using Service.Exceptions.CPMServiceExceptions;
using Service.Interface;
using Service.Models;

namespace Service;

public class CpmService : ICpmService
{
    private const double SLACK_TOLERANCE = 0.0001;

    public CpmResult CalculateCriticalPath(List<TaskDTO> tasks)
    {
        if (tasks == null) throw new NullTaskListException();

        if (tasks.Count == 0) throw new EmptyTaskListException();

        CalculateEarlyDates(tasks);
        CalculateLateDates(tasks);
        CalculateSlackAndCriticalTasks(tasks);

        var criticalPath = FindCriticalPath(tasks);
        var projectDuration = CalculateProjectDuration(tasks);

        return new CpmResult
        {
            AllTasks = tasks,
            CriticalPath = criticalPath,
            CriticalTasks = tasks.Where(t => t.IsCritical).ToList(),
            ProjectDuration = projectDuration
        };
    }

    private void CalculateEarlyDates(List<TaskDTO> tasks)
    {
        var processedTasks = new HashSet<TaskDTO>();
        var remainingTasks = new Queue<TaskDTO>(tasks);
        var iterationCount = 0;
        var maxIterations = tasks.Count * tasks.Count;

        while (remainingTasks.Count > 0)
        {
            iterationCount++;
            if (iterationCount > maxIterations) throw new CircularDependencyException();

            var task = remainingTasks.Dequeue();

            if (task.PreviousTasks.All(p => processedTasks.Contains(p)))
            {
                if (task.PreviousTasks.Count == 0)
                    task.StartDate = task.ExpectedStartDate;
                else
                    task.StartDate = task.PreviousTasks.Max(p => p.EndDate);

                task.EndDate = task.StartDate.AddDays(task.Duration);
                processedTasks.Add(task);
            }
            else
            {
                remainingTasks.Enqueue(task);
            }
        }
    }

    private void CalculateLateDates(List<TaskDTO> tasks)
    {
        var finalTasks = tasks.Where(t => !IsSuccessorOfAny(t, tasks)).ToList();
        var projectEndDate = finalTasks.Max(t => t.EndDate);
        foreach (var finalTask in finalTasks)
        {
            finalTask.LatestFinish = projectEndDate;
            finalTask.LatestStart = finalTask.LatestFinish.AddDays(-finalTask.Duration);
        }

        var processedTasks = new HashSet<TaskDTO>(finalTasks);
        var remainingTasks = new Queue<TaskDTO>(tasks.Except(finalTasks));
        var iterationCount = 0;
        var maxIterations = tasks.Count * tasks.Count;

        while (remainingTasks.Count > 0)
        {
            iterationCount++;
            if (iterationCount > maxIterations) throw new CircularDependencyException();

            var task = remainingTasks.Dequeue();
            var successors = GetSuccessors(task, tasks);

            if (successors.All(s => processedTasks.Contains(s)))
            {
                if (successors.Count > 0)
                    task.LatestFinish = successors.Min(s => s.LatestStart);
                else
                    task.LatestFinish = projectEndDate;

                task.LatestStart = task.LatestFinish.AddDays(-task.Duration);
                processedTasks.Add(task);
            }
            else
            {
                remainingTasks.Enqueue(task);
            }
        }
    }

    private void CalculateSlackAndCriticalTasks(List<TaskDTO> tasks)
    {
        foreach (var task in tasks)
        {
            task.Slack = task.LatestStart - task.StartDate;
            task.IsCritical = Math.Abs(task.Slack.TotalDays) < SLACK_TOLERANCE;
        }
    }

    private List<TaskDTO> FindCriticalPath(List<TaskDTO> tasks)
    {
        var criticalPath = new List<TaskDTO>();
        var criticalTasks = tasks.Where(t => t.IsCritical).ToList();

        if (!criticalTasks.Any())
            throw new CriticalPathCalculationException("No critical tasks were found in the project");
        var processedTasks = new HashSet<TaskDTO>();
        var initialCriticalTasks = criticalTasks
            .Where(t => t.PreviousTasks.Count == 0 || !t.PreviousTasks.Any(p => p.IsCritical))
            .OrderBy(t => t.StartDate)
            .ToList();
        if (!initialCriticalTasks.Any()) initialCriticalTasks.Add(criticalTasks.OrderBy(t => t.StartDate).First());
        foreach (var initialTask in initialCriticalTasks)
        {
            if (processedTasks.Contains(initialTask))
                continue;

            var currentTask = initialTask;
            while (currentTask != null)
            {
                if (processedTasks.Contains(currentTask))
                    break;

                criticalPath.Add(currentTask);
                processedTasks.Add(currentTask);

                var nextTask = criticalTasks.FirstOrDefault(t =>
                    t.PreviousTasks.Contains(currentTask) &&
                    t.IsCritical &&
                    !processedTasks.Contains(t));

                currentTask = nextTask;
            }
        }

        return criticalPath;
    }

    private bool IsSuccessorOfAny(TaskDTO task, List<TaskDTO> allTasks)
    {
        return allTasks.Any(t => t.PreviousTasks.Contains(task));
    }

    private List<TaskDTO> GetSuccessors(TaskDTO task, List<TaskDTO> allTasks)
    {
        return allTasks.Where(t => t.PreviousTasks.Contains(task)).ToList();
    }

    private int CalculateProjectDuration(List<TaskDTO> tasks)
    {
        if (!tasks.Any())
            return 0;

        var earliestStart = tasks.Min(t => t.StartDate);
        var latestFinish = tasks.Max(t => t.EndDate);

        return (int)(latestFinish - earliestStart).TotalDays;
    }

    public DateTime CalculateEarlyStart(TaskDTO task)
    {
        if (task.PreviousTasks == null || task.PreviousTasks.Count == 0) return task.ExpectedStartDate;

        return task.PreviousTasks.Max(t => t.EndDate);
    }

    public DateTime CalculateEarlyFinish(TaskDTO task)
    {
        return task.StartDate.AddDays(task.Duration);
    }

    public DateTime CalculateLateStart(TaskDTO task, List<TaskDTO> allTasks)
    {
        var successors = GetSuccessors(task, allTasks);

        if (!successors.Any())
        {
            if (task.LatestStart != default) return task.LatestStart;

            if (task.StartDate != default) return task.StartDate;

            return task.ExpectedStartDate;
        }

        var earliestSuccessorStart = successors.Min(s => s.LatestStart);
        return earliestSuccessorStart.AddDays(-task.Duration);
    }

    public DateTime CalculateLateFinish(TaskDTO task, List<TaskDTO> allTasks)
    {
        var successors = GetSuccessors(task, allTasks);

        if (!successors.Any())
        {
            var finalTasks = allTasks.Where(t => !IsSuccessorOfAny(t, allTasks)).ToList();
            return finalTasks.Max(t => t.EndDate);
        }

        return successors.Min(s => s.LatestStart);
    }

    public bool IsCritical(TaskDTO task)
    {
        return Math.Abs(task.Slack.TotalDays) < SLACK_TOLERANCE;
    }
}

public class CpmResult
{
    public List<TaskDTO> AllTasks { get; set; }
    public List<TaskDTO> CriticalPath { get; set; }
    public List<TaskDTO> CriticalTasks { get; set; }
    public int ProjectDuration { get; set; }
}