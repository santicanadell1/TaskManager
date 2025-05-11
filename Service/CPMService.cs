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

            var task = tasks.First();
            task.IsCritical = true;
            
            return new CpmResult
            {
                AllTasks = tasks,
                CriticalPath = new List<Task> { task },
                CriticalTasks = new List<Task> { task },
                ProjectDuration = task.Duration
            };
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