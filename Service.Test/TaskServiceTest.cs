using DataAccess;
using DataAccess.ProjectRepositoryExceptions;
using Domain;
using Domain.Exceptions.TaskRepositoryExceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = Domain.Task;

namespace Service.Test
{
    [TestClass]
    public class TaskServiceTest
    {
        private InMemoryDatabase _database;
        private TaskService _taskService;
        private Project _genericProject;
        private Task _task1;
        private Task _task2;
        private TaskDTO _taskDTO1;
        private TaskDTO _taskDTO2;
        private Resource _resource1;
        private Resource _resource2;
        private ResourceDTO _resourceDTO1;
        private ResourceDTO _resourceDTO2;

        [TestInitialize]
        public void Setup()
        {
            _database = new InMemoryDatabase();
            _taskService = new TaskService(_database);

            _genericProject = new Project("Generic Project", "Description", DateTime.Now);
            _database.projects.AddProject(_genericProject);

            _resource1 = new Resource("Resource 1", "Type 1", "Description 1") { Id = 1 };
            _resource2 = new Resource("Resource 2", "Type 2", "Description 2") { Id = 2 };

            _resourceDTO1 = new ResourceDTO
            {
                Id = 1,
                Name = "Resource 1",
                Type = "Type 1",
                Description = "Description 1"
            };

            _resourceDTO2 = new ResourceDTO
            {
                Id = 2,
                Name = "Resource 2",
                Type = "Type 2",
                Description = "Description 2"
            };

            _taskDTO1 = new TaskDTO
            {
                Title = "Task 1",
                Description = "Description of Task 1",
                ExpectedStartDate = DateTime.Now,
                Duration = 5,
                PreviousTasks = new List<TaskDTO>(),
                SameTimeTasks = new List<TaskDTO>(),
                Resources = new List<ResourceDTO> { _resourceDTO1 },
                State = StateDTO.TODO
            };

            _taskDTO2 = new TaskDTO
            {
                Title = "Task 2",
                Description = "Description of Task 2",
                ExpectedStartDate = DateTime.Now.AddDays(2),
                Duration = 3,
                PreviousTasks = new List<TaskDTO>(),
                SameTimeTasks = new List<TaskDTO>(),
                Resources = new List<ResourceDTO> { _resourceDTO2 },
                State = StateDTO.DOING
            };

            _task1 = new Task(
                _taskDTO1.Title,
                _taskDTO1.Description,
                _taskDTO1.ExpectedStartDate,
                _taskDTO1.Duration,
                new List<Task>(),
                new List<Task>(),
                new List<Resource> { _resource1 }
            );
            _task1.Id = 1;

            _task2 = new Task(
                _taskDTO2.Title,
                _taskDTO2.Description,
                _taskDTO2.ExpectedStartDate,
                _taskDTO2.Duration,
                new List<Task>(),
                new List<Task>(),
                new List<Resource> { _resource2 }
            );
            _task2.Id = 2;

            _database.projects.AddTask("Generic Project", _task1);
            _database.projects.AddTask("Generic Project", _task2);
        }

        [TestMethod]
        public void AddTask_ShouldAddTask_WhenTaskIsValid()
        {
            var taskDTO = new TaskDTO
            {
                Title = "Test Task",
                Description = "Test Description",
                ExpectedStartDate = DateTime.Now.AddDays(1),
                Duration = 5,
                PreviousTasks = new List<TaskDTO>(),
                SameTimeTasks = new List<TaskDTO>(),
                Resources = new List<ResourceDTO>()
            };

            _taskService.AddTask("Generic Project", taskDTO);

            var project = _database.projects.GetProject(p => p.Name == "Generic Project");
            var tasks = project.Tasks;
            Assert.AreEqual(3, tasks.Count);
            Assert.AreEqual("Test Task", tasks[2].Title);
        }

        [TestMethod]
        [ExpectedException(typeof(ProjectNotFoundException))]
        public void AddTask_ShouldThrowException_WhenProjectDoesNotExist()
        {
            var taskDTO = new TaskDTO
            {
                Title = "Test Task",
                Description = "Test Description",
                ExpectedStartDate = DateTime.Now.AddDays(1),
                Duration = 5
            };

            _taskService.AddTask("Non-Existent Project", taskDTO);
        }
        
        [TestMethod]
        public void AddTask_ShouldAddTaskWithPreviousTasks_WhenPreviousTasksExist()
        {
            var taskDTO = new TaskDTO
            {
                Title = "Task with Dependencies",
                Description = "Description",
                ExpectedStartDate = DateTime.Now.AddDays(5),
                Duration = 2,
                PreviousTasks = new List<TaskDTO>
                {
                    new TaskDTO { Id = _task1.Id },
                    new TaskDTO { Id = _task2.Id }
                },
                SameTimeTasks = new List<TaskDTO>(),
                Resources = new List<ResourceDTO>()
            };

            _taskService.AddTask("Generic Project", taskDTO);

            var project = _database.projects.GetProject(p => p.Name == "Generic Project");
            var addedTask = project.Tasks.FirstOrDefault(t => t.Title == "Task with Dependencies");

            Assert.IsNotNull(addedTask);
            Assert.AreEqual(2, addedTask.PreviousTasks.Count);
            Assert.IsTrue(addedTask.PreviousTasks.Any(t => t.Id == _task1.Id));
            Assert.IsTrue(addedTask.PreviousTasks.Any(t => t.Id == _task2.Id));
        }

        [TestMethod]
        public void AddTask_ShouldAddTaskWithSameTimeTasks_WhenSameTimeTasksExist()
        {
            var taskDTO = new TaskDTO
            {
                Title = "Task with Same Time Tasks",
                Description = "Description",
                ExpectedStartDate = DateTime.Now,
                Duration = 2,
                PreviousTasks = new List<TaskDTO>(),
                SameTimeTasks = new List<TaskDTO>
                {
                    new TaskDTO { Id = _task1.Id },
                    new TaskDTO { Id = _task2.Id }
                },
                Resources = new List<ResourceDTO>()
            };

            _taskService.AddTask("Generic Project", taskDTO);

            var project = _database.projects.GetProject(p => p.Name == "Generic Project");
            var addedTask = project.Tasks.FirstOrDefault(t => t.Title == "Task with Same Time Tasks");

            Assert.IsNotNull(addedTask);
            Assert.AreEqual(2, addedTask.SameTimeTasks.Count);
            Assert.IsTrue(addedTask.SameTimeTasks.Any(t => t.Id == _task1.Id));
            Assert.IsTrue(addedTask.SameTimeTasks.Any(t => t.Id == _task2.Id));
        }

        [TestMethod]
        public void DeleteTask_ShouldDeleteTask_WhenTaskExists()
        {
            _taskService.DeleteTask("Generic Project", _task1.Id);

            var project = _database.projects.GetProject(p => p.Name == "Generic Project");
            Assert.AreEqual(1, project.Tasks.Count);
            Assert.AreEqual("Task 2", project.Tasks[0].Title);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ProjectNotFoundException))]
        public void DeleteTask_ShouldThrowException_WhenProjectDoesNotExist()
        {
            _taskService.DeleteTask("Non-Existent Project", _task1.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskNotFoundException))]
        public void DeleteTask_ShouldThrowException_WhenTaskDoesNotExist()
        {
            _taskService.DeleteTask("Generic Project", 999);
        }


        [TestMethod]
        public void UpdateTask_ShouldUpdateTask_WhenTaskExists()
        {
            var updateDTO = new TaskDTO
            {
                Title = "Updated Task 1",
                Description = "Updated Description",
                ExpectedStartDate = DateTime.Now.AddDays(1),
                Duration = 6,
                PreviousTasks = new List<TaskDTO>(),
                SameTimeTasks = new List<TaskDTO>(),
                State = StateDTO.TODO,
                Resources = new List<ResourceDTO>()
            };

            _taskService.UpdateTask("Generic Project", _task1.Id, updateDTO);

            var project = _database.projects.GetProject(p => p.Name == "Generic Project");
            var updatedTask = project.Tasks.FirstOrDefault(t => t.Id == _task1.Id);

            Assert.AreEqual("Updated Task 1", updatedTask.Title);
            Assert.AreEqual("Updated Description", updatedTask.Description);
            Assert.AreEqual(6, updatedTask.Duration);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ProjectNotFoundException))]
        public void UpdateTask_ShouldThrowException_WhenProjectDoesNotExist()
        {
            var updateDTO = new TaskDTO
            {
                Title = "Updated Task",
                Description = "Updated Description",
                ExpectedStartDate = DateTime.Now,
                Duration = 5
            };

            _taskService.UpdateTask("Non-Existent Project", _task1.Id, updateDTO);
        }
        
        [TestMethod]
        [ExpectedException(typeof(TaskNotFoundException))]
        public void UpdateTask_ShouldThrowException_WhenTaskDoesNotExist()
        {
            var updateDTO = new TaskDTO
            {
                Title = "Updated Task",
                Description = "Updated Description",
                ExpectedStartDate = DateTime.Now,
                Duration = 5
            };

            _taskService.UpdateTask("Generic Project", 999, updateDTO);
            
            
        }
        
        [TestMethod]
        public void UpdateTask_ShouldUpdateTaskWithSameTimeTasks()
        {
            var updateDTO = new TaskDTO
            {
                Title = "Updated Task with Same Time",
                Description = "Description",
                ExpectedStartDate = DateTime.Now,
                Duration = 3,
                PreviousTasks = new List<TaskDTO>(),
                SameTimeTasks = new List<TaskDTO> { new TaskDTO { Id = _task2.Id } },
                Resources = new List<ResourceDTO>()
            };

            _taskService.UpdateTask("Generic Project", _task1.Id, updateDTO);

            var project = _database.projects.GetProject(p => p.Name == "Generic Project");
            var updatedTask = project.Tasks.FirstOrDefault(t => t.Id == _task1.Id);

            Assert.AreEqual(1, updatedTask.SameTimeTasks.Count);
            Assert.AreEqual(_task2.Id, updatedTask.SameTimeTasks[0].Id);
        }
        
        [TestMethod]
        public void UpdateTask_ShouldUpdateTaskWithResources()
        {
            var updateDTO = new TaskDTO
            {
                Title = "Updated Task with Resources",
                Description = "Description",
                ExpectedStartDate = DateTime.Now,
                Duration = 3,
                PreviousTasks = new List<TaskDTO>(),
                SameTimeTasks = new List<TaskDTO>(),
                Resources = new List<ResourceDTO>
                {
                    new ResourceDTO { Id = 10, Name = "Updated Resource", Type = "Updated Type", Description = "Updated Desc" }
                }
            };

            _taskService.UpdateTask("Generic Project", _task1.Id, updateDTO);

            var project = _database.projects.GetProject(p => p.Name == "Generic Project");
            var updatedTask = project.Tasks.FirstOrDefault(t => t.Id == _task1.Id);

            Assert.AreEqual(1, updatedTask.Resources.Count);
            Assert.AreEqual("Updated Resource", updatedTask.Resources[0].Name);
        }
        
        [TestMethod]
public void UpdateTask_ShouldIgnoreSelfInPreviousTasks()
{
    var updateDTO = new TaskDTO
    {
        Title = "Updated Task Self Reference",
        Description = "Description",
        ExpectedStartDate = DateTime.Now,
        Duration = 3,
        PreviousTasks = new List<TaskDTO> { new TaskDTO { Id = _task1.Id } }, 
        SameTimeTasks = new List<TaskDTO>(),
        Resources = new List<ResourceDTO>()
    };

    _taskService.UpdateTask("Generic Project", _task1.Id, updateDTO);

    var project = _database.projects.GetProject(p => p.Name == "Generic Project");
    var updatedTask = project.Tasks.FirstOrDefault(t => t.Id == _task1.Id);

    Assert.AreEqual(0, updatedTask.PreviousTasks.Count);
}

        [TestMethod]
        public void UpdateTask_ShouldIgnoreSelfInSameTimeTasks()
        {
            var updateDTO = new TaskDTO
            {
                Title = "Updated Task Self Reference",
                Description = "Description",
                ExpectedStartDate = DateTime.Now,
                Duration = 3,
                PreviousTasks = new List<TaskDTO>(),
                SameTimeTasks = new List<TaskDTO> { new TaskDTO { Id = _task1.Id } }, 
                Resources = new List<ResourceDTO>()
            };

            _taskService.UpdateTask("Generic Project", _task1.Id, updateDTO);

            var project = _database.projects.GetProject(p => p.Name == "Generic Project");
            var updatedTask = project.Tasks.FirstOrDefault(t => t.Id == _task1.Id);

            Assert.AreEqual(0, updatedTask.SameTimeTasks.Count);
        }

        [TestMethod]
        public void GetTasks_ShouldReturnAllTasks_WhenProjectExists()
        {
            var tasks = _taskService.GetTasks("Generic Project");

            Assert.AreEqual(2, tasks.Count);
            Assert.AreEqual("Task 1", tasks[0].Title);
            Assert.AreEqual("Task 2", tasks[1].Title);
        }

        [TestMethod]
        public void GetTask_ShouldReturnTask_WhenTaskExists()
        {
            var taskDTO = _taskService.GetTask("Generic Project", _task1.Id);

            Assert.AreEqual("Task 1", taskDTO.Title);
            Assert.AreEqual("Description of Task 1", taskDTO.Description);
            Assert.AreEqual(5, taskDTO.Duration);
        }
        [TestMethod]
        [ExpectedException(typeof(ProjectNotFoundException))]
        public void GetTasks_ShouldThrowException_WhenProjectDoesNotExist()
        {
            _taskService.GetTasks("Non-Existent Project");
        }

        [TestMethod]
        public void GetTasks_ShouldReturnTasksWithResources()
        {
            var tasks = _taskService.GetTasks("Generic Project");

            Assert.AreEqual(1, tasks[0].Resources.Count);
            Assert.AreEqual("Resource 1", tasks[0].Resources[0].Name);
    
            Assert.AreEqual(1, tasks[1].Resources.Count);
            Assert.AreEqual("Resource 2", tasks[1].Resources[0].Name);
        }
        [TestMethod]
        public void GetTasks_ShouldReturnTasksWithCorrectState()
        {
            var tasks = _taskService.GetTasks("Generic Project");

            Assert.AreEqual(StateDTO.TODO, tasks[0].State);
            Assert.AreEqual(StateDTO.TODO, tasks[1].State);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ProjectNotFoundException))]
        public void GetTask_ShouldThrowException_WhenProjectDoesNotExist()
        {
            _taskService.GetTask("Non-Existent Project", _task1.Id);
        }
        
        [TestMethod]
        [ExpectedException(typeof(TaskNotFoundException))]
        public void GetTask_ShouldThrowException_WhenTaskDoesNotExist()
        {
            _taskService.GetTask("Generic Project", 999);
        }
        
        [TestMethod]
        public void GetTask_ShouldReturnTaskWithResources()
        {
            var taskDTO = _taskService.GetTask("Generic Project", _task1.Id);

            Assert.AreEqual(1, taskDTO.Resources.Count);
            Assert.AreEqual("Resource 1", taskDTO.Resources[0].Name);
            Assert.AreEqual("Type 1", taskDTO.Resources[0].Type);
            Assert.AreEqual("Description 1", taskDTO.Resources[0].Description);
        }

        [TestMethod]
        public void GetTask_ShouldReturnTaskWithCorrectState()
        {
            var taskDTO = _taskService.GetTask("Generic Project", _task1.Id);
            Assert.AreEqual(StateDTO.TODO, taskDTO.State);
    
            var taskDTO2 = _taskService.GetTask("Generic Project", _task2.Id);
            Assert.AreEqual(StateDTO.TODO, taskDTO2.State);
        }
        
        [TestMethod]
        public void GetTask_ShouldReturnTaskWithMinimalPreviousTasks()
        {
            var taskWithDependencies = new Task(
                "Task with Dependencies",
                "Description",
                DateTime.Now,
                3,
                new List<Task> { _task1 },
                new List<Task>(),
                new List<Resource>()
            );
            taskWithDependencies.Id = 3;
    
            _database.projects.AddTask("Generic Project", taskWithDependencies);
    
            var taskDTO = _taskService.GetTask("Generic Project", taskWithDependencies.Id);
    
            Assert.AreEqual(1, taskDTO.PreviousTasks.Count);
            Assert.AreEqual(_task1.Id, taskDTO.PreviousTasks[0].Id);
            Assert.AreEqual(_task1.Title, taskDTO.PreviousTasks[0].Title);
            Assert.IsNull(taskDTO.PreviousTasks[0].Description); 
        }
        
        [TestMethod]
        public void GetTask_ShouldCorrectlyMapTaskProperties_TestingFromEntity()
        {
            var task = new Task(
                "Mapping Test Task",
                "Detailed description for mapping test",
                new DateTime(2025, 1, 1),
                7,
                new List<Task>(),
                new List<Task>(),
                new List<Resource> {
                    new Resource("Mapping Resource", "Mapping Type", "Mapping Description") { Id = 50 }
                }
            );
    
            task.State = Domain.State.DONE;
    
            _database.projects.AddTask("Generic Project", task);
    
            var project = _database.projects.GetProject(p => p.Name == "Generic Project");
            var addedTask = project.Tasks.FirstOrDefault(t => t.Title == "Mapping Test Task");
    
            Assert.IsNotNull(addedTask, "La tarea no se agregó correctamente al proyecto");
    
            var taskDTO = _taskService.GetTask("Generic Project", addedTask.Id);
    
            Assert.AreEqual(addedTask.Id, taskDTO.Id);
            Assert.AreEqual("Mapping Test Task", taskDTO.Title);
            Assert.AreEqual("Detailed description for mapping test", taskDTO.Description);
            Assert.AreEqual(new DateTime(2025, 1, 1), taskDTO.ExpectedStartDate);
            Assert.AreEqual(7, taskDTO.Duration);
            Assert.AreEqual(StateDTO.DONE, taskDTO.State);
            Assert.AreEqual(1, taskDTO.Resources.Count);
            Assert.AreEqual("Mapping Resource", taskDTO.Resources[0].Name);
            Assert.AreEqual("Mapping Type", taskDTO.Resources[0].Type);
            Assert.AreEqual("Mapping Description", taskDTO.Resources[0].Description);
            Assert.AreEqual(50, taskDTO.Resources[0]);
        }

    }
}
