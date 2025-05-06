using DataAccess;
using Domain;
using Service.Models;
using Task = Domain.Task;


namespace Service
{
    public class TaskService
    {
        private readonly InMemoryDatabase _database;

        public TaskService(InMemoryDatabase database)
        {
            _database = database;
        }

        public DateTime CalculateEarlyStart(Task task)
        {
            if (task.PreviousTasks == null || task.PreviousTasks.Count == 0)
            {
                return task.ExpectedStartDate;
            }

            DateTime latestPreviousEnd = task.PreviousTasks.Max(t => t.EndDate);
            return latestPreviousEnd;
        }


        public DateTime CalculateEarlyFinish(Task task)
        {
            return task.ExpectedStartDate.AddDays(task.Duration);
        }


        public DateTime CalculateLateStart(Task task)
        {
            if (task.PreviousTasks.Count == 0)
            {
                return task.ExpectedStartDate;
            }

            DateTime latestPreviousEnd = task.PreviousTasks.Max(t => t.EndDate);
            return latestPreviousEnd.AddDays(0);
        }

        public DateTime CalculateLateFinish(Task task)
        {
            if (task.PreviousTasks.Count == 0)
            {
                return CalculateEarlyFinish(task);
            }

            DateTime latestPreviousFinish = task.PreviousTasks.Max(t => t.EndDate);
            return latestPreviousFinish.AddDays(task.Duration);
        }

        public bool IsCritical(Task task)
        {
            return CalculateEarlyStart(task) == CalculateLateStart(task) &&
                   CalculateEarlyFinish(task) == CalculateLateFinish(task);
        }


        private TaskDTO FromEntity(Task task)
        {
            return new TaskDTO()
            {
                Title = task.Title,
                Description = task.Description,
                ExpectedStartDate = task.ExpectedStartDate,
                Duration = task.Duration,
                PreviousTasks = task.PreviousTasks ?? new List<Task>(),
                SameTimeTasks = task.SameTimeTasks ?? new List<Task>(),
                State = task.State
            };
        }


        private Task ToEntity(TaskDTO taskDTO)
        {
            return new Task(
                taskDTO.Title,
                taskDTO.Description,
                taskDTO.ExpectedStartDate,
                taskDTO.Duration,
                taskDTO.PreviousTasks ?? new List<Task>(),
                taskDTO.SameTimeTasks ?? new List<Task>(),
                taskDTO.Resources ?? new List<Resource>()
            );
        }
    }
}