using Domain;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using Service.Models;
using Task = Domain.Task;

namespace Service
{
    public class CpmService
    {
        private const double SLACK_TOLERANCE = 0.0001;

        public CpmResult CalculateCriticalPath(List<TaskDTO> tasks)
        {
            if (tasks == null)
            {
                throw new NullTaskListException();
            }

            if (tasks.Count == 0)
            {
                throw new EmptyTaskListException();
            }

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
            int iterationCount = 0;
            int maxIterations = tasks.Count * tasks.Count;

            while (remainingTasks.Count > 0)
            {
                iterationCount++;
                if (iterationCount > maxIterations)
                {
                    throw new CircularDependencyException();
                }

                var task = remainingTasks.Dequeue();

                if (task.PreviousTasks.All(p => processedTasks.Contains(p)))
                {
                    if (task.PreviousTasks.Count == 0)
                    {
                        task.StartDate = task.ExpectedStartDate;
                    }
                    else
                    {
                        task.StartDate = task.PreviousTasks.Max(p => p.EndDate);
                    }

                    task.EndDate = task.StartDate.AddDays(task.Duration);
                    processedTasks.Add(task);
                }
                else
                {
                    remainingTasks.Enqueue(task);
                }
            }
        }

        private void CalculateLateDates(List<Task> tasks)
        {
            var finalTasks = tasks.Where(t => !IsSuccessorOfAny(t, tasks)).ToList();

            foreach (var finalTask in finalTasks)
            {
                finalTask.LatestFinish = finalTask.EndDate;
                finalTask.LatestStart = finalTask.LatestFinish.AddDays(-finalTask.Duration);
            }

            var processedTasks = new HashSet<Task>(finalTasks);
            var remainingTasks = new Queue<Task>(tasks.Except(finalTasks));
            int iterationCount = 0;
            int maxIterations = tasks.Count * tasks.Count;

            while (remainingTasks.Count > 0)
            {
                iterationCount++;
                if (iterationCount > maxIterations)
                {
                    throw new CircularDependencyException();
                }

                var task = remainingTasks.Dequeue();
                var successors = GetSuccessors(task, tasks);

                if (successors.All(s => processedTasks.Contains(s)))
                {
                    if (successors.Count > 0)
                    {
                        task.LatestFinish = successors.Min(s => s.LatestStart);
                    }
                    else
                    {
                        task.LatestFinish = task.EndDate;
                    }

                    task.LatestStart = task.LatestFinish.AddDays(-task.Duration);
                    processedTasks.Add(task);
                }
                else
                {
                    remainingTasks.Enqueue(task);
                }
            }
        }

        private void CalculateSlackAndCriticalTasks(List<Task> tasks)
        {
            foreach (var task in tasks)
            {
                task.Slack = task.LatestStart - task.StartDate;
                task.IsCritical = Math.Abs(task.Slack.TotalDays) < SLACK_TOLERANCE;
            }
        }

        private List<Task> FindCriticalPath(List<Task> tasks)
        {
            var criticalPath = new List<Task>();
            var criticalTasks = tasks.Where(t => t.IsCritical).ToList();

            if (!criticalTasks.Any())
            {
                throw new CriticalPathCalculationException("No critical tasks were found in the project");
            }

            var currentTask = criticalTasks.FirstOrDefault(t => 
                t.PreviousTasks.Count == 0 || 
                !t.PreviousTasks.Any(p => p.IsCritical));

            if (currentTask == null)
            {
                currentTask = criticalTasks.OrderBy(t => t.StartDate).First();
            }

            var processedTasks = new HashSet<Task>();

            while (currentTask != null)
            {
                if (processedTasks.Contains(currentTask))
                {
                    break;
                }
                
                criticalPath.Add(currentTask);
                processedTasks.Add(currentTask);
                
                var nextTask = criticalTasks.FirstOrDefault(t => 
                    t.PreviousTasks.Contains(currentTask) && 
                    t.IsCritical && 
                    !processedTasks.Contains(t));
                    
                currentTask = nextTask;
            }

            return criticalPath;
        }

        private bool IsSuccessorOfAny(Task task, List<Task> allTasks)
        {
            return allTasks.Any(t => t.PreviousTasks.Contains(task));
        }

        private List<Task> GetSuccessors(Task task, List<Task> allTasks)
        {
            return allTasks.Where(t => t.PreviousTasks.Contains(task)).ToList();
        }

        private int CalculateProjectDuration(List<Task> tasks)
        {
            if (!tasks.Any())
                return 0;

            var earliestStart = tasks.Min(t => t.StartDate);
            var latestFinish = tasks.Max(t => t.EndDate);

            return (int)(latestFinish - earliestStart).TotalDays;
        }

        public DateTime CalculateEarlyStart(Task task)
        {
            if (task.PreviousTasks == null || task.PreviousTasks.Count == 0)
            {
                return task.ExpectedStartDate;
            }

            return task.PreviousTasks.Max(t => t.EndDate);
        }

        public DateTime CalculateEarlyFinish(Task task)
        {
            return task.StartDate.AddDays(task.Duration);
        }

        public DateTime CalculateLateStart(Task task, List<Task> allTasks)
        {
            var successors = GetSuccessors(task, allTasks);
            
            if (!successors.Any())
            {
                if (task.LatestStart != default(DateTime))
                {
                    return task.LatestStart;
                }
                else if (task.StartDate != default(DateTime))
                {
                    return task.StartDate;
                }
                else
                {
                    return task.ExpectedStartDate;
                }
            }

            DateTime earliestSuccessorStart = successors.Min(s => s.LatestStart);
            return earliestSuccessorStart.AddDays(-task.Duration);
        }

        public DateTime CalculateLateFinish(Task task, List<Task> allTasks)
        {
            var successors = GetSuccessors(task, allTasks);
            
            if (!successors.Any())
            {
                return task.EndDate;
            }

            return successors.Min(s => s.LatestStart);
        }

        public bool IsCritical(Task task)
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
}