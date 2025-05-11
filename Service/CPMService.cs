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
            
            return new CpmResult
            {
                AllTasks = tasks,
                CriticalPath = new List<Task>(tasks),
                CriticalTasks = new List<Task>(tasks),
                ProjectDuration = tasks.First().Duration
            };
        }

        private void CalculateEarlyDates(List<Task> tasks)
        {
            foreach (var task in tasks)
            {
                if (task.PreviousTasks.Count == 0)
                {
                    task.StartDate = task.ExpectedStartDate;
                    task.EndDate = task.StartDate.AddDays(task.Duration);
                }
            }
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