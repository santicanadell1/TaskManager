using Domain;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
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

            return null;
        }
    }

    public class CpmResult
    {
    }
}