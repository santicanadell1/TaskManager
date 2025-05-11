using Domain;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = Domain.Task;

namespace Service
{
    public class CpmService
    {
        public CpmResult CalculateCriticalPath(List<Task> tasks)
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
            
            var projectDuration = CalculateProjectDuration(tasks);
            
            return new CpmResult
            {
                AllTasks = tasks,
                CriticalPath = new List<Task>(tasks.Where(t => t.IsCritical)),
                CriticalTasks = new List<Task>(tasks.Where(t => t.IsCritical)),
                ProjectDuration = projectDuration
            };
        }

        private void CalculateEarlyDates(List<Task> tasks)
        {
            var processedTasks = new HashSet<Task>();
            var remainingTasks = new Queue<Task>(tasks);

            while (remainingTasks.Count > 0)
            {
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

            while (remainingTasks.Count > 0)
            {
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
                task.IsCritical = Math.Abs(task.Slack.TotalDays) < 0.0001;
            }
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
    }

    public class CpmResult
    {
        public List<Task> AllTasks { get; set; }
        public List<Task> CriticalPath { get; set; }
        public List<Task> CriticalTasks { get; set; }
        public int ProjectDuration { get; set; }
    }
}