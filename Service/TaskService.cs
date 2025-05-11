using DataAccess;
using DataAccess.ProjectRepositoryExceptions;
using Domain;
using Domain.Exceptions.TaskRepositoryExceptions;
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

        public void AddTask(string projectName, TaskDTO taskDTO)
        {
            var project = _database.projects.GetProject(p => p.Name == projectName);
            if (project == null)
            {
                throw new ProjectNotFoundException();
            }

            var previousTasks = new List<Task>();
            var sameTimeTasks = new List<Task>();

            if (taskDTO.PreviousTasks != null)
            {
                foreach (var prevTaskDTO in taskDTO.PreviousTasks)
                {
                    if (prevTaskDTO.Id.HasValue)
                    {
                        var existingTask = project.Tasks.FirstOrDefault(t => t.Id == prevTaskDTO.Id);
                        if (existingTask != null)
                        {
                            previousTasks.Add(existingTask);
                        }
                    }
                }
            }

            if (taskDTO.SameTimeTasks != null)
            {
                foreach (var sameTaskDTO in taskDTO.SameTimeTasks)
                {
                    if (sameTaskDTO.Id.HasValue)
                    {
                        var existingTask = project.Tasks.FirstOrDefault(t => t.Id == sameTaskDTO.Id);
                        if (existingTask != null)
                        {
                            sameTimeTasks.Add(existingTask);
                        }
                    }
                }
            }

            var task = new Task(
                taskDTO.Title,
                taskDTO.Description,
                taskDTO.ExpectedStartDate,
                taskDTO.Duration,
                previousTasks,
                sameTimeTasks,
                ToResourceEntityList(taskDTO.Resources)
            );

            _database.projects.AddTask(projectName, task);
        }

        public void DeleteTask(string projectName, int? taskId)
        {
            var project = _database.projects.GetProject(p => p.Name == projectName);
            if (project == null)
            {
                throw new ProjectNotFoundException();
            }

            var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                throw new TaskNotFoundException();
            }

            _database.projects.RemoveTask(projectName, task.Id);
        }

        public void UpdateTask(string projectName, int? taskId, TaskDTO taskDTO)
        {
            var project = _database.projects.GetProject(p => p.Name == projectName);
            if (project == null)
            {
                throw new ProjectNotFoundException();
            }

            var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                throw new TaskNotFoundException();
            }

            var previousTasks = new List<Task>();
            if (taskDTO.PreviousTasks != null)
            {
                foreach (var prevTaskDTO in taskDTO.PreviousTasks)
                {
                    if (prevTaskDTO.Id.HasValue)
                    {
                        var existingTask = project.Tasks.FirstOrDefault(t => t.Id == prevTaskDTO.Id);
                        if (existingTask != null && existingTask.Id != taskId)
                        {
                            previousTasks.Add(existingTask);
                        }
                    }
                }
            }

            var sameTimeTasks = new List<Task>();
            if (taskDTO.SameTimeTasks != null)
            {
                foreach (var sameTaskDTO in taskDTO.SameTimeTasks)
                {
                    if (sameTaskDTO.Id.HasValue)
                    {
                        var existingTask = project.Tasks.FirstOrDefault(t => t.Id == sameTaskDTO.Id);
                        if (existingTask != null && existingTask.Id != taskId)
                        {
                            sameTimeTasks.Add(existingTask);
                        }
                    }
                }
            }

            var updatedTask = new Task(
                taskDTO.Title,
                taskDTO.Description,
                taskDTO.ExpectedStartDate,
                taskDTO.Duration,
                previousTasks,
                sameTimeTasks,
                ToResourceEntityList(taskDTO.Resources)
            );
            updatedTask.Id = task.Id;

            _database.projects.UpdateTask(projectName, taskId, updatedTask);
        }

        public List<TaskDTO> GetTasks(string projectName)
        {
            var project = _database.projects.GetProject(p => p.Name == projectName);
            if (project == null)
            {
                throw new ProjectNotFoundException();
            }

            var taskDTOs = project.Tasks.Select(t => new TaskDTO
            {
                Title = t.Title,
                Description = t.Description,
                ExpectedStartDate = t.ExpectedStartDate,
                Duration = t.Duration,
                PreviousTasks = FromEntityList(t.PreviousTasks),
                SameTimeTasks = FromEntityList(t.SameTimeTasks),
                State = (StateDTO)t.State,
                Resources = FromResourceEntityList(t.Resources),
                Id = t.Id
            }).ToList();

            return taskDTOs;
        }

        public TaskDTO GetTask(string projectName, int? taskId)
        {
            var project = _database.projects.GetProject(p => p.Name == projectName);
            if (project == null)
            {
                throw new ProjectNotFoundException();
            }

            var task = project.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                throw new TaskNotFoundException();
            }

            return FromEntity(task);
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
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                ExpectedStartDate = task.ExpectedStartDate,
                Duration = task.Duration,
                PreviousTasks = ToMinimalTaskDTOList(task.PreviousTasks),
                SameTimeTasks = ToMinimalTaskDTOList(task.SameTimeTasks),
                State = (StateDTO)task.State,
                Resources = FromResourceEntityList(task.Resources) ?? new List<ResourceDTO>()
            };
        }

        private List<TaskDTO> ToMinimalTaskDTOList(List<Task> tasks)
        {
            if (tasks == null)
            {
                return new List<TaskDTO>();
            }

            return tasks.Select(t => new TaskDTO
            {
                Id = t.Id,
                Title = t.Title
            }).ToList();
        }

        private List<TaskDTO> FromEntityList(List<Task> tasks)
        {
            if (tasks == null)
            {
                return new List<TaskDTO>();
            }

            var taskDTOs = new List<TaskDTO>();
            foreach (var task in tasks)
            {
                taskDTOs.Add(new TaskDTO
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    ExpectedStartDate = task.ExpectedStartDate,
                    Duration = task.Duration,
                    State = (StateDTO)task.State,
                    PreviousTasks = new List<TaskDTO>(),
                    SameTimeTasks = new List<TaskDTO>(),
                    Resources = new List<ResourceDTO>()
                });
            }

            return taskDTOs;
        }

        private List<ResourceDTO> FromResourceEntityList(List<Resource> resources)
        {
            var resourceDTOs = new List<ResourceDTO>();
            foreach (var resource in resources)
            {
                resourceDTOs.Add(new ResourceDTO
                {
                    Name = resource.Name,
                    Type = resource.Type,
                    Description = resource.Description,
                    Id = resource.Id
                });
            }

            return resourceDTOs;
        }

        private Task ToEntity(TaskDTO taskDTO)
        {
            return new Task(
                taskDTO.Title,
                taskDTO.Description,
                taskDTO.ExpectedStartDate,
                taskDTO.Duration,
                ToEntityList(taskDTO.PreviousTasks),
                ToEntityList(taskDTO.SameTimeTasks),
                ToResourceEntityList(taskDTO.Resources)
            );
        }

        private List<Task> ToEntityList(List<TaskDTO> taskDTOs)
        {
            if (taskDTOs == null)
            {
                return new List<Task>();
            }

            var tasks = new List<Task>();
            foreach (var taskDTO in taskDTOs)
            {
                tasks.Add(ToEntity(taskDTO));
            }

            return tasks;
        }

        private List<Resource> ToResourceEntityList(List<ResourceDTO> resourceDTOs)
        {
            if (resourceDTOs == null)
            {
                return new List<Resource>();
            }

            var resources = new List<Resource>();
            foreach (var resourceDTO in resourceDTOs)
            {
                resources.Add(new Resource(resourceDTO.Name, resourceDTO.Type, resourceDTO.Description)
                    { Id = resourceDTO.Id });
            }

            return resources;
        }
    }
}