using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Domain.Exceptions.TaskExceptions;
using Service.Converter;
using Service.Exceptions.ResourceServiceExceptions;
using Service.Models;
using Task = Domain.Task;

namespace Service.Test;

[TestClass]
public class TaskServiceTest
{
    private AppDbContext _context;
    private CpmService _cpmService;
    private Project _genericProject;
    private Login _login;
    private IRepositoryManager _repositoryManager;
    private Resource _resource1;
    private Resource _resource2;
    private ResourceDTO _resourceDTO1;
    private ResourceDTO _resourceDTO2;
    private ResourceService _resourceService;
    private Task _task1;
    private Task _task2;
    private TaskDTO _taskDTO1;
    private TaskDTO _taskDTO2;
    private TaskService _taskService;
    private UserService _userService;
    private UserDTO userDTO;

    [TestInitialize]
    public void Setup()
    {
        var contextFactory = new InMemoryAppContextFactory();
        _context = contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repositoryManager = new RepositoryManager(_context);

        _cpmService = new CpmService();
        _taskService = new TaskService(_repositoryManager, _cpmService);
        _login = new Login(_repositoryManager);
        _resourceService = new ResourceService(_repositoryManager);

        _genericProject = new Project("Generic Project", "Description", DateTime.Parse("2025-06-16"));
        _repositoryManager.ProjectRepository.Add(_genericProject);

        _userService = new UserService(_repositoryManager);
        userDTO = new UserDTO
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin.user@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };
        _userService.AddUser(userDTO);
        _login.LoginUser(userDTO.Email, userDTO.Password);

        _resourceDTO1 = new ResourceDTO
        {
            Name = "Resource 1",
            Type = "Type 1",
            Description = "Description 1"
        };
        _resourceDTO2 = new ResourceDTO
        {
            Name = "Resource 2",
            Type = "Type 2",
            Description = "Description 2"
        };

        _resourceService.AddResource(_resourceDTO1);
        _resourceService.AddResource(_resourceDTO2);
        _resourceDTO1.Id = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource 1").Id;
        _resourceDTO2.Id = _repositoryManager.ResourceRepository.Get(r => r.Name == "Resource 2").Id;

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
        )
        {
            State = State.TODO
        };
        _task2 = new Task(
            _taskDTO2.Title,
            _taskDTO2.Description,
            _taskDTO2.ExpectedStartDate,
            _taskDTO2.Duration,
            new List<Task>(),
            new List<Task>(),
            new List<Resource> { _resource2 }
        )
        {
            State = State.DOING
        };

        _taskService.AddTask("Generic Project", _taskDTO1);
        _taskService.AddTask("Generic Project", _taskDTO2);
        _context.SaveChanges();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Database.EnsureDeleted();
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

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "Generic Project");
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
        var taskDTO1 = _taskService.GetTask("Generic Project",
            _repositoryManager.TaskRepository.Get(t => t.Title == "Task 1").Title);
        var taskDTO2 = _taskService.GetTask("Generic Project",
            _repositoryManager.TaskRepository.Get(t => t.Title == "Task 2").Title);

        var taskDTO = new TaskDTO
        {
            Title = "Task with Dependencies",
            Description = "Description",
            ExpectedStartDate = DateTime.Now.AddDays(5),
            Duration = 2,
            PreviousTasks = new List<TaskDTO> { taskDTO1, taskDTO2 },
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO>()
        };
        _taskService.AddTask("Generic Project", taskDTO);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "Generic Project");
        var addedTask = project.Tasks.FirstOrDefault(t => t.Title == "Task with Dependencies");

        Assert.IsNotNull(addedTask);
        Assert.AreEqual(2, addedTask.PreviousTasks.Count);
        Assert.IsTrue(addedTask.PreviousTasks.Any(t => t.Title == "Task 1"));
        Assert.IsTrue(addedTask.PreviousTasks.Any(t => t.Title == "Task 2"));
    }

    [TestMethod]
    public void AddTask_ShouldAddTaskWithSameTimeTasks_WhenSameTimeTasksExist()
    {
        var taskDTO1 = _taskService.GetTask("Generic Project",
            _repositoryManager.TaskRepository.Get(t => t.Title == "Task 1").Title);
        var taskDTO2 = _taskService.GetTask("Generic Project",
            _repositoryManager.TaskRepository.Get(t => t.Title == "Task 2").Title);

        var taskDTO = new TaskDTO
        {
            Title = "Task with Same Time Tasks",
            Description = "Description",
            ExpectedStartDate = DateTime.Now,
            Duration = 2,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO> { taskDTO1, taskDTO2 },
            Resources = new List<ResourceDTO>()
        };
        _taskService.AddTask("Generic Project", taskDTO);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "Generic Project");
        var addedTask = project.Tasks.FirstOrDefault(t => t.Title == "Task with Same Time Tasks");

        Assert.IsNotNull(addedTask);
        Assert.AreEqual(2, addedTask.SameTimeTasks.Count);
        Assert.IsTrue(addedTask.SameTimeTasks.Any(t => t.Title == "Task 1"));
        Assert.IsTrue(addedTask.SameTimeTasks.Any(t => t.Title == "Task 2"));
    }

    [TestMethod]
    public void DeleteTask_ShouldDeleteTask_WhenTaskExists()
    {
        var Task1 = _repositoryManager.TaskRepository.Get(t => t.Title == "Task 1");
        _taskService.DeleteTask("Generic Project", Task1.Title);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "Generic Project");
        Assert.AreEqual(1, project.Tasks.Count);
        Assert.AreEqual("Task 2", project.Tasks[0].Title);
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void DeleteTask_ShouldThrowException_WhenProjectDoesNotExist()
    {
        _taskService.DeleteTask("Non-Existent Project", _task1.Title);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void DeleteTask_ShouldThrowException_WhenTaskDoesNotExist()
    {
        _taskService.DeleteTask("Generic Project", "Non-Existent Task");
    }

    [TestMethod]
    public void UpdateTask_ShouldUpdateTask_WhenTaskExists()
    {
        var Task1 = _repositoryManager.TaskRepository.Get(t => t.Title == "Task 1");
        var updateDTO = new TaskDTO
        {
            Title = "Updated Task 1",
            Description = "Updated Description",
            ExpectedStartDate = DateTime.Now.AddDays(1),
            Duration = 6,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>(),
            Id = Task1.Id
        };
        _taskService.UpdateTask("Generic Project", Task1.Title, updateDTO);

        var updatedTask = _taskService.GetTask("Generic Project", Task1.Title);
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
        _taskService.UpdateTask("Non-Existent Project", _task1.Title, updateDTO);
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
        _taskService.UpdateTask("Generic Project", "taSK", updateDTO);
    }

    [TestMethod]
    public void UpdateTask_ShouldUpdateTaskWithSameTimeTasks()
    {
        var Task2 = _repositoryManager.TaskRepository.Get(t => t.Title == "Task 2");
        var Task2DTO = _taskService.GetTask("Generic Project", Task2.Title);
        var updateDTO = new TaskDTO
        {
            Title = "Updated Task with Same Time",
            Description = "Description",
            ExpectedStartDate = DateTime.Now,
            Duration = 3,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO> { Task2DTO },
            Resources = new List<ResourceDTO>()
        };
        var Task1 = _repositoryManager.TaskRepository.Get(t => t.Title == "Task 1");
        _taskService.UpdateTask("Generic Project", Task1.Title, updateDTO);

        var updatedTask = _taskService.GetTask("Generic Project", Task1.Title);
        Assert.AreEqual(1, updatedTask.SameTimeTasks.Count);
        Assert.AreEqual(Task2.Id, updatedTask.SameTimeTasks[0].Id);
    }

    [TestMethod]
    public void UpdateTask_ShouldUpdateTaskWithResources()
    {
        var Task1 = _repositoryManager.TaskRepository.Get(t => t.Title == "Task 1");
        var newResourceDTO = new ResourceDTO
            { Name = "Updated Resource", Type = "Updated Type", Description = "Updated Desc" };
        _resourceService.AddResource(newResourceDTO);
        var UpdatedResource = _repositoryManager.ResourceRepository.Get(r => r.Name == "Updated Resource");
        var UpdatedResourceDTO = _resourceService.Get(UpdatedResource.Id);

        var updateDTO = new TaskDTO
        {
            Title = "Updated Task with Resources",
            Description = "Description",
            ExpectedStartDate = DateTime.Now,
            Duration = 3,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { UpdatedResourceDTO }
        };
        _taskService.UpdateTask("Generic Project", Task1.Title, updateDTO);

        var updatedTask = _taskService.GetTask("Generic Project", Task1.Title);
        Assert.AreEqual(1, updatedTask.Resources.Count);
        Assert.AreEqual("Updated Resource", updatedTask.Resources[0].Name);
    }

    [TestMethod]
    public void UpdateTask_ShouldIgnoreSelfInPreviousTasks()
    {
        var Task1 = _repositoryManager.TaskRepository.Get(t => t.Title == "Task 1");
        var Task1DTO = _taskService.GetTask("Generic Project", Task1.Title);
        var updateDTO = new TaskDTO
        {
            Title = "Updated Task Self Reference",
            Description = "Description",
            ExpectedStartDate = DateTime.Now,
            Duration = 3,
            PreviousTasks = new List<TaskDTO> { Task1DTO },
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO>(),
            Id = Task1.Id
        };
        _taskService.UpdateTask("Generic Project", Task1.Title, updateDTO);

        var updatedTask = _taskService.GetTask("Generic Project", Task1.Title);
        Assert.AreEqual(0, updatedTask.PreviousTasks.Count);
    }

    [TestMethod]
    public void UpdateTask_ShouldIgnoreSelfInSameTimeTasks()
    {
        var Task1 = _repositoryManager.TaskRepository.Get(t => t.Title == "Task 1");
        var Task1DTO = _taskService.GetTask("Generic Project", Task1.Title);
        var updateDTO = new TaskDTO
        {
            Title = "Updated Task Self Reference",
            Description = "Description",
            ExpectedStartDate = DateTime.Now,
            Duration = 3,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO> { Task1DTO },
            Resources = new List<ResourceDTO>()
        };
        _taskService.UpdateTask("Generic Project", Task1.Title, updateDTO);

        var updatedTask = _taskService.GetTask("Generic Project", Task1.Title);
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
        var Task1 = _repositoryManager.TaskRepository.Get(t => t.Title == "Task 1");
        var taskDTO = _taskService.GetTask("Generic Project", Task1.Title);
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
        Assert.AreEqual(StateDTO.DOING, tasks[1].State);
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void GetTask_ShouldThrowException_WhenProjectDoesNotExist()
    {
        _taskService.GetTask("Non-Existent Project", _task1.Title);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void GetTask_ShouldThrowException_WhenTaskDoesNotExist()
    {
        _taskService.GetTask("Generic Project", "Non-Existent Task");
    }

    [TestMethod]
    public void GetTask_ShouldReturnTaskWithResources()
    {
        var Task1 = _repositoryManager.TaskRepository.Get(t => t.Title == "Task 1");
        var taskDTO = _taskService.GetTask("Generic Project", Task1.Title);
        Assert.AreEqual(1, taskDTO.Resources.Count);
        Assert.AreEqual("Resource 1", taskDTO.Resources[0].Name);
        Assert.AreEqual("Type 1", taskDTO.Resources[0].Type);
        Assert.AreEqual("Description 1", taskDTO.Resources[0].Description);
    }

    [TestMethod]
    public void GetTask_ShouldReturnTaskWithCorrectState()
    {
        var Task1 = _repositoryManager.TaskRepository.Get(t => t.Title == "Task 1");
        var taskDTO = _taskService.GetTask("Generic Project", Task1.Title);
        Assert.AreEqual(StateDTO.TODO, taskDTO.State);
        var Task2 = _repositoryManager.TaskRepository.Get(t => t.Title == "Task 2");
        var taskDTO2 = _taskService.GetTask("Generic Project", Task2.Title);
        Assert.AreEqual(StateDTO.DOING, taskDTO2.State);
    }

    [TestMethod]
    public void GetTask_ShouldReturnTaskWithMinimalPreviousTasks()
    {
        var Task1 = _repositoryManager.TaskRepository.Get(t => t.Title == "Task 1");
        var taskWithDependencies = new Task(
            "Task with Dependencies",
            "Description",
            DateTime.Now,
            3,
            new List<Task> { Task1 },
            new List<Task>(),
            new List<Resource>()
        );
        _repositoryManager.TaskRepository.Add(taskWithDependencies);
        _repositoryManager.ProjectRepository.AddTask("Generic Project", taskWithDependencies);
        var Task2 = _repositoryManager.TaskRepository.Get(t => t.Title == "Task with Dependencies");

        var taskDTO = _taskService.GetTask("Generic Project", Task2.Title);
        Assert.AreEqual(1, taskDTO.PreviousTasks.Count);
        Assert.AreEqual(_task1.Title, taskDTO.PreviousTasks[0].Title);
        Assert.IsNull(taskDTO.PreviousTasks[0].Description);
    }

    [TestMethod]
    public void GetTask_ShouldCorrectlyMapTaskProperties_TestingFromEntity()
    {
        var mappingTask = new Task(
            "Mapping Test Task",
            "Detailed description for mapping test",
            new DateTime(2025, 1, 1),
            7,
            new List<Task>(),
            new List<Task>(),
            new List<Resource> { new("Mapping Resource", "Mapping Type", "Mapping Description") }
        )
        {
            State = State.DONE
        };
        _repositoryManager.ProjectRepository.AddTask("Generic Project", mappingTask);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "Generic Project");
        var addedTask = project.Tasks.FirstOrDefault(t => t.Title == "Mapping Test Task");
        Assert.IsNotNull(addedTask, "La tarea no se agregó correctamente al proyecto");

        var taskDTO = _taskService.GetTask("Generic Project", addedTask.Title);
        Assert.AreEqual("Mapping Test Task", taskDTO.Title);
        Assert.AreEqual("Detailed description for mapping test", taskDTO.Description);
        Assert.AreEqual(new DateTime(2025, 1, 1), taskDTO.ExpectedStartDate);
        Assert.AreEqual(7, taskDTO.Duration);
        Assert.AreEqual(StateDTO.DONE, taskDTO.State);
        Assert.AreEqual(1, taskDTO.Resources.Count);
        Assert.AreEqual("Mapping Resource", taskDTO.Resources[0].Name);
        Assert.AreEqual("Mapping Type", taskDTO.Resources[0].Type);
        Assert.AreEqual("Mapping Description", taskDTO.Resources[0].Description);
    }

    [TestMethod]
    public void AddTask_ShouldCorrectlyMapDTOToEntity_TestingToEntity()
    {
        var resourceDto = _resourceService.Get(1);
        var complexDTO = new TaskDTO
        {
            Title = "Complex Mapping DTO",
            Description = "Complex mapping description",
            ExpectedStartDate = new DateTime(2026, 6, 15),
            Duration = 9,
            State = StateDTO.DOING,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { resourceDto }
        };
        _taskService.AddTask("Generic Project", complexDTO, true);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "Generic Project");
        var addedTask = project.Tasks.FirstOrDefault(t => t.Title == "Complex Mapping DTO");
        Assert.IsNotNull(addedTask);
        Assert.AreEqual("Complex Mapping DTO", addedTask.Title);
        Assert.AreEqual("Complex mapping description", addedTask.Description);
        Assert.AreEqual(9, addedTask.Duration);
        Assert.AreEqual(1, addedTask.Resources.Count);
        Assert.AreEqual(resourceDto.Name, addedTask.Resources[0].Name);
        Assert.AreEqual(resourceDto.Type, addedTask.Resources[0].Type);
        Assert.AreEqual(resourceDto.Description, addedTask.Resources[0].Description);
    }

    [TestMethod]
    public void AddTaskAndGetTask_ShouldPreserveListRelationships_TestingEntityListMappings()
    {
        var taskX = new TaskDTO
        {
            Title = "Task X",
            Description = "Task X Description",
            ExpectedStartDate = DateTime.Now,
            Duration = 3,
            Resources = new List<ResourceDTO>()
        };
        _taskService.AddTask("Generic Project", taskX);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "Generic Project");
        var taskXEntity = _taskService.GetTask("Generic Project", taskX.Title);

        var taskY = new TaskDTO
        {
            Title = "Task Y",
            Description = "Task Y Description",
            ExpectedStartDate = DateTime.Now.AddDays(3),
            Duration = 2,
            PreviousTasks = new List<TaskDTO> { taskXEntity },
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO>()
        };
        _taskService.AddTask("Generic Project", taskY);

        var taskYEntity = project.Tasks.FirstOrDefault(t => t.Title == "Task Y");
        Assert.AreEqual(1, taskYEntity.PreviousTasks.Count);
        Assert.AreEqual(taskXEntity.Title, taskYEntity.PreviousTasks[0].Title);

        var taskYDTO = _taskService.GetTask("Generic Project", taskYEntity.Title);
        Assert.AreEqual(1, taskYDTO.PreviousTasks.Count);
        Assert.AreEqual(taskXEntity.Title, taskYDTO.PreviousTasks[0].Title);
    }

    [TestMethod]
    public void UpdateTask_WithComplexRelationships_ShouldCorrectlyMapAllProperties()
    {
        var taskA = new Task("Task A", "Description A", DateTime.Now, 1, new List<Task>(), new List<Task>(),
            new List<Resource>());
        var taskB = new Task("Task B", "Description B", DateTime.Now, 2, new List<Task>(), new List<Task>(),
            new List<Resource>());
        var taskC = new Task("Task C", "Description C", DateTime.Now, 3, new List<Task>(), new List<Task>(),
            new List<Resource>());

        _repositoryManager.TaskRepository.Add(taskA);
        _repositoryManager.TaskRepository.Add(taskB);
        _repositoryManager.TaskRepository.Add(taskC);
        _context.SaveChanges();

        _repositoryManager.ProjectRepository.AddTask("Generic Project", taskA);
        _repositoryManager.ProjectRepository.AddTask("Generic Project", taskB);
        _repositoryManager.ProjectRepository.AddTask("Generic Project", taskC);

        var addedTaskC = _taskService.GetTask("Generic Project", taskC.Title);
        Assert.IsNotNull(addedTaskC, "La tarea C no se agregó correctamente");

        _repositoryManager.ResourceRepository.Add(new Resource
            { Name = "Complex Update Resource", Type = "Update Type", Description = "Update Resource Description" });
        _context.SaveChanges();
        var resourceEntity = _repositoryManager.ResourceRepository.Get(r => r.Name == "Complex Update Resource");

        var updateDTO = new TaskDTO
        {
            Title = "Updated Task C",
            Description = "Updated Description C",
            ExpectedStartDate = DateTime.Now.AddDays(5),
            Duration = 4,
            PreviousTasks = new List<TaskDTO> { new() { Id = taskA.Id } },
            SameTimeTasks = new List<TaskDTO> { new() { Id = taskB.Id } },
            Resources = new List<ResourceDTO> { new() { Id = resourceEntity.Id } }
        };
        _taskService.UpdateTask("Generic Project", taskC.Title, updateDTO);

        var updatedTaskC = _taskService.GetTask("Generic Project", updateDTO.Title);
        Assert.IsNotNull(updatedTaskC);
        Assert.AreEqual("Updated Task C", updatedTaskC.Title);
        Assert.AreEqual("Updated Description C", updatedTaskC.Description);
        Assert.AreEqual(4, updatedTaskC.Duration);
        Assert.AreEqual(1, updatedTaskC.PreviousTasks.Count);
        Assert.AreEqual(taskA.Title, updatedTaskC.PreviousTasks[0].Title);
        Assert.AreEqual(1, updatedTaskC.SameTimeTasks.Count);
        Assert.AreEqual(taskB.Title, updatedTaskC.SameTimeTasks[0].Title);
        Assert.AreEqual(1, updatedTaskC.Resources.Count);
        Assert.AreEqual("Complex Update Resource", updatedTaskC.Resources[0].Name);
    }

    [TestMethod]
    public void GetTasks_ShouldCorrectlyMapAllTaskProperties_TestingFromEntityList()
    {
        var specialTask = new Task(
            "Special Task for List Mapping",
            "Special Description",
            new DateTime(2025, 12, 25),
            5,
            new List<Task>(),
            new List<Task>(),
            new List<Resource> { new("Special Resource", "Special Type", "Special Description") }
        )
        {
            State = State.DONE
        };
        _repositoryManager.ProjectRepository.AddTask("Generic Project", specialTask);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "Generic Project");
        var addedSpecialTask = project.Tasks.FirstOrDefault(t => t.Title == "Special Task for List Mapping");
        Assert.IsNotNull(addedSpecialTask, "La tarea no se agregó correctamente al proyecto");

        var allTasks = _taskService.GetTasks("Generic Project");
        var mappedSpecialTask = allTasks.FirstOrDefault(t => t.Id == addedSpecialTask.Id);

        Assert.IsNotNull(mappedSpecialTask);
        Assert.AreEqual("Special Task for List Mapping", mappedSpecialTask.Title);
        Assert.AreEqual("Special Description", mappedSpecialTask.Description);
        Assert.AreEqual(new DateTime(2025, 12, 25), mappedSpecialTask.ExpectedStartDate);
        Assert.AreEqual(5, mappedSpecialTask.Duration);
        Assert.AreEqual(StateDTO.DONE, mappedSpecialTask.State);
        Assert.AreEqual(1, mappedSpecialTask.Resources.Count);
        Assert.AreEqual("Special Resource", mappedSpecialTask.Resources[0].Name);
    }

    [TestMethod]
    public void GetTask_ShouldReturnTaskWithCpmProperties()
    {
        var task1Entity = _repositoryManager.TaskRepository.Get(t => t.Title == "Task 1");
        var taskDTO = _taskService.GetTask("Generic Project", task1Entity.Title);

        var maxSlack = TimeSpan.FromMilliseconds(1);
        Assert.IsTrue(taskDTO.Slack >= TimeSpan.Zero && taskDTO.Slack <= maxSlack,
            $"El valor de Slack no está dentro del rango esperado: {taskDTO.Slack}");
    }

    [TestMethod]
    [ExpectedException(typeof(TaskException))]
    public void AddTask_ShouldThrowException_WhenTaskStartDateIsBeforeProjectsOne()
    {
        var taskDTO = new TaskDTO
        {
            Title = "Task with invalid start date",
            Description = "Description",
            ExpectedStartDate = DateTime.Parse("2025-06-16").AddDays(-2),
            Duration = 3,
            Resources = new List<ResourceDTO>()
        };
        _taskService.AddTask("Generic Project", taskDTO);
    }

    [TestMethod]
    public void AddTask_ShouldRescheduleTask_WhenResourceInUseAndSolveTrue()
    {
        var originalStart = _taskDTO1.ExpectedStartDate.Date;
        var existingDuration = _taskDTO1.Duration;

        var toSchedule = new TaskDTO
        {
            Title = "ResolvedTask",
            Description = "Will be auto-rescheduled",
            ExpectedStartDate = originalStart,
            Duration = 3,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { _resourceDTO1 },
            State = StateDTO.TODO
        };
        _taskService.AddTask("Generic Project", toSchedule, true);

        var scheduled = _taskService.GetTask("Generic Project", "ResolvedTask");
        var expectedRescheduled = originalStart.AddDays(existingDuration);
        Assert.AreEqual(expectedRescheduled.Date, scheduled.ExpectedStartDate.Date);
    }

    [TestMethod]
    [ExpectedException(typeof(ResourceNotAvailableException))]
    public void AddTask_ShouldThrowResourceNotAvailable_WhenResourceInUseAndSolveFalse()
    {
        var conflictingTask = new TaskDTO
        {
            Title = "ConflictTask",
            Description = "Conflicts with existing",
            ExpectedStartDate = _taskDTO1.ExpectedStartDate,
            Duration = 3,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { _resourceDTO1 },
            State = StateDTO.TODO
        };
        _taskService.AddTask("Generic Project", conflictingTask);
    }

    [TestMethod]
    public void UpdateTask_ShouldRescheduleTask_WhenResourceInUseAndSolveTrue()
    {
        var resourceInUse = _resourceDTO1;
        var originalStart = _taskDTO1.ExpectedStartDate.Date;
        var task1Duration = _taskDTO1.Duration;
        var updateDTO = new TaskDTO
        {
            Title = "Task 2 Rescheduled",
            Description = "Auto-reschedule on Resource 1",
            ExpectedStartDate = originalStart,
            Duration = 3,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { resourceInUse },
            State = StateDTO.DOING
        };
        _taskService.UpdateTask("Generic Project", "Task 2", updateDTO, true);

        var updated = _taskService.GetTask("Generic Project", "Task 2 Rescheduled");
        var expected = originalStart.AddDays(task1Duration);
        Assert.AreEqual(expected.Date, updated.ExpectedStartDate.Date);
    }

    [TestMethod]
    [ExpectedException(typeof(ResourceNotAvailableException))]
    public void UpdateTask_ShouldThrowResourceNotAvailable_WhenResourceInUseAndSolveFalse()
    {
        var resourceInUse = _resourceDTO1;
        var overlapStart = _taskDTO1.ExpectedStartDate.Date;
        var updateDTO = new TaskDTO
        {
            Title = "Task 2 Updated",
            Description = "Now uses Resource 1 and overlaps",
            ExpectedStartDate = overlapStart,
            Duration = 3,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO> { resourceInUse },
            State = StateDTO.DOING
        };
        _taskService.UpdateTask("Generic Project", "Task 2", updateDTO);
    }

    [TestMethod]
    public void ToMinimalTaskDTOList_ShouldReturnEmptyList_WhenTasksIsNull()
    {
        var taskConverter = new TaskConverter(_repositoryManager);
        var result = taskConverter.ToMinimalTaskDTOList(null);
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void ConvertToEntityList_ShouldReturnEmptyList_WhenTaskDTOsIsNull()
    {
        var taskConverter = new TaskConverter(_repositoryManager);
        var result = taskConverter.ConvertToEntityList(null);
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void ConvertToEntityList_ShouldConvertAllTasks_WhenTaskDTOsProvided()
    {
        var taskConverter = new TaskConverter(_repositoryManager);
        var taskDTOs = new List<TaskDTO>
        {
            new()
            {
                Title = "Convert Task 1",
                Description = "First task to convert",
                ExpectedStartDate = DateTime.Now,
                Duration = 2,
                State = StateDTO.TODO,
                Resources = new List<ResourceDTO>()
            },
            new()
            {
                Title = "Convert Task 2",
                Description = "Second task to convert",
                ExpectedStartDate = DateTime.Now.AddDays(1),
                Duration = 3,
                State = StateDTO.DOING,
                Resources = new List<ResourceDTO>()
            }
        };
        var result = taskConverter.ConvertToEntityList(taskDTOs);
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Convert Task 1", result[0].Title);
        Assert.AreEqual("Convert Task 2", result[1].Title);
        Assert.AreEqual(State.TODO, result[0].State);
        Assert.AreEqual(State.DOING, result[1].State);
    }

    [TestMethod]
    public void ConvertFromEntityList_ShouldReturnEmptyList_WhenTasksIsNull()
    {
        var taskConverter = new TaskConverter(_repositoryManager);
        var result = taskConverter.ConvertFromEntityList(null);
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void ConvertFromEntityList_ShouldConvertAllTasks_WhenTasksProvided()
    {
        var taskConverter = new TaskConverter(_repositoryManager);
        var tasks = new List<Task>
        {
            new("Entity Task 1", "First entity task", DateTime.Now, 2, new List<Task>(), new List<Task>(),
                new List<Resource>()),
            new("Entity Task 2", "Second entity task", DateTime.Now.AddDays(1), 3, new List<Task>(), new List<Task>(),
                new List<Resource>())
        };
        tasks[0].State = State.TODO;
        tasks[1].State = State.DOING;
        var result = taskConverter.ConvertFromEntityList(tasks);
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Entity Task 1", result[0].Title);
        Assert.AreEqual("Entity Task 2", result[1].Title);
        Assert.AreEqual(StateDTO.TODO, result[0].State);
        Assert.AreEqual(StateDTO.DOING, result[1].State);
    }

    [TestMethod]
    public void GetExistingTasksFromIds_ShouldHandleTasksWithoutIds()
    {
        var taskConverter = new TaskConverter(_repositoryManager);
        var taskDTOsWithoutIds = new List<TaskDTO>
        {
            new() { Id = null, Title = "Task Without ID", Description = "This task has no ID" }
        };
        var result = taskConverter.GetExistingTasksFromIds(taskDTOsWithoutIds);
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetExistingTasksFromIds_ShouldHandleNonExistentIds()
    {
        var taskConverter = new TaskConverter(_repositoryManager);
        var taskDTOsWithInvalidIds = new List<TaskDTO>
        {
            new()
            {
                Id = 999999, Title = "Task With Invalid ID", Description = "This task has an ID that doesn't exist"
            }
        };
        var result = taskConverter.GetExistingTasksFromIds(taskDTOsWithInvalidIds);
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }
}