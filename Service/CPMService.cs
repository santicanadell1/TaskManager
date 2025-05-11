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

            foreach (var task in tasks)
            {
                task.IsCritical = true;
            }
            
            var projectDuration = CalculateProjectDuration(tasks);
            
            return new CpmResult
            {
                AllTasks = tasks,
                CriticalPath = new List<Task>(tasks),
                CriticalTasks = new List<Task>(tasks),
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