using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Domain.Exceptions.TaskExceptions;
using Service.Models;
using Task = Domain.Task;

namespace Service.Test;

[TestClass]
public class TaskServiceTest
{
    private CpmService _cpmService;
    private AppDbContext _context;
    private Project _genericProject;
    private Login _login;
    private Resource _resource1;
    private Resource _resource2;
    private ResourceDTO _resourceDTO1;
    private ResourceDTO _resourceDTO2;
    private Task _task1;
    private Task _task2;
    private TaskDTO _taskDTO1;
    private TaskDTO _taskDTO2;
    private TaskService _taskService;
    private UserService _userService;
    private UserDTO userDTO;
    private ProjectRepository _projectRepository;
    private NotificationRepository _notificationRepository;
    private UserRepository _userRepository;
    private TaskRepository _taskRepository;
    private ResourceRepository _resourceRepository;
    private ResourceService _resourceService;

    [TestInitialize]
    public void Setup()
    {
        var contextFactory = new InMemoryAppContextFactory();
        _context = contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _projectRepository = new ProjectRepository(_context);
        _notificationRepository = new NotificationRepository(_context);
        _userRepository = new UserRepository(_context);
        _taskRepository = new TaskRepository(_context);
        _resourceRepository = new ResourceRepository(_context);

        _cpmService = new CpmService();
        _taskService = new TaskService(_projectRepository, _notificationRepository, _userRepository, _cpmService,
            _taskRepository, _resourceRepository);
        _login = new Login(_userRepository);
        _resourceService = new ResourceService(_resourceRepository,_projectRepository);

        _genericProject = new Project("Generic Project", "Description", DateTime.Now);
        _projectRepository.Add(_genericProject);
        _userService = new UserService(_userRepository);
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
        _resourceDTO1.Id = _resourceRepository.Get(r => r.Name == "Resource 1").Id;
        _resourceDTO2.Id = _resourceRepository.Get(r => r.Name == "Resource 2").Id;
        

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
        _task1.State = State.TODO;
        
        
        _task2 = new Task(
            _taskDTO2.Title,
            _taskDTO2.Description,
            _taskDTO2.ExpectedStartDate,
            _taskDTO2.Duration,
            new List<Task>(),
            new List<Task>(),
            new List<Resource> { _resource2 }
        );
        _task2.State = State.DOING;
        
        _taskService.AddTask("Generic Project",_taskDTO1);
        _taskService.AddTask("Generic Project",_taskDTO2);
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

        var project = _projectRepository.Get(p => p.Name == "Generic Project");
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
        var taskDTO1 = _taskService.GetTask("Generic Project",_taskRepository.Get(t=> t.Title == "Task 1").Title);;
        var taskDTO2 = _taskService.GetTask("Generic Project",_taskRepository.Get(t=> t.Title == "Task 2").Title);;
        var taskDTO = new TaskDTO
        {
            Title = "Task with Dependencies",
            Description = "Description",
            ExpectedStartDate = DateTime.Now.AddDays(5),
            Duration = 2,
            PreviousTasks = new List<TaskDTO>
            {
                taskDTO1,taskDTO2 
            },
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO>()
        };
        
        _taskService.AddTask("Generic Project", taskDTO);

        var project = _projectRepository.Get(p => p.Name == "Generic Project");
        var addedTask = project.Tasks.FirstOrDefault(t => t.Title == "Task with Dependencies");

        Assert.IsNotNull(addedTask);
        Assert.AreEqual(2, addedTask.PreviousTasks.Count);
        Assert.IsTrue(addedTask.PreviousTasks.Any(t => t.Title == "Task 1"));
        Assert.IsTrue(addedTask.PreviousTasks.Any(t => t.Title == "Task 2"));
    }

    [TestMethod]
    public void AddTask_ShouldAddTaskWithSameTimeTasks_WhenSameTimeTasksExist()
    {
        var taskDTO1 = _taskService.GetTask("Generic Project",_taskRepository.Get(t=> t.Title == "Task 1").Title);;
        var taskDTO2 = _taskService.GetTask("Generic Project",_taskRepository.Get(t=> t.Title == "Task 2").Title);;
        var taskDTO = new TaskDTO
        {
            Title = "Task with Same Time Tasks",
            Description = "Description",
            ExpectedStartDate = DateTime.Now,
            Duration = 2,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>
            {
                taskDTO1,
                taskDTO2
            },
            Resources = new List<ResourceDTO>()
        };

        _taskService.AddTask("Generic Project", taskDTO);

        var project = _projectRepository.Get(p => p.Name == "Generic Project");
        var addedTask = project.Tasks.FirstOrDefault(t => t.Title == "Task with Same Time Tasks");

        Assert.IsNotNull(addedTask);
        Assert.AreEqual(2, addedTask.SameTimeTasks.Count);
        Assert.IsTrue(addedTask.SameTimeTasks.Any(t => t.Title == "Task 1"));
        Assert.IsTrue(addedTask.SameTimeTasks.Any(t => t.Title == "Task 2"));
    }

    [TestMethod]
    public void DeleteTask_ShouldDeleteTask_WhenTaskExists()
    {
        var Task1 = _taskRepository.Get(t => t.Title == "Task 1");
        _taskService.DeleteTask("Generic Project", Task1.Title);

        var project = _projectRepository.Get(p => p.Name == "Generic Project");
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
        var Task1 = _taskRepository.Get(t => t.Title == "Task 1");
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

        _taskService.UpdateTask("Generic Project","taSK", updateDTO);
    }

    [TestMethod]
    public void UpdateTask_ShouldUpdateTaskWithSameTimeTasks()
    {
        var Task2 = _taskRepository.Get(t => t.Title == "Task 2");
        var Task2DTO = _taskService.GetTask("Generic Project", Task2.Title);
        var updateDTO = new TaskDTO
        {
            Title = "Updated Task with Same Time",
            Description = "Description",
            ExpectedStartDate = DateTime.Now,
            Duration = 3,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO> { Task2DTO},
            Resources = new List<ResourceDTO>()
        };
        var Task1 = _taskRepository.Get(t => t.Title == "Task 1");
        _taskService.UpdateTask("Generic Project", Task1.Title, updateDTO);
        
        var updatedTask = _taskService.GetTask("Generic Project", Task1.Title);

        Assert.AreEqual(1, updatedTask.SameTimeTasks.Count);
        Assert.AreEqual(Task2.Id, updatedTask.SameTimeTasks[0].Id);
    }

    [TestMethod]
    public void UpdateTask_ShouldUpdateTaskWithResources()
    {
        var Task1 = _taskRepository.Get(t => t.Title == "Task 1");
        _resourceService.AddResource(new() { Name = "Updated Resource", Type = "Updated Type", Description = "Updated Desc" });
        var UpdatedResource = _resourceRepository.Get(r=> r.Name == "Updated Resource");
        var UpdatedResourceDTO = _resourceService.Get(UpdatedResource.Id);
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
                UpdatedResourceDTO
            }
        };

        _taskService.UpdateTask("Generic Project", Task1.Title, updateDTO);
        
        var updatedTask = _taskService.GetTask("Generic Project", Task1.Title);

        Assert.AreEqual(1, updatedTask.Resources.Count);
        Assert.AreEqual("Updated Resource", updatedTask.Resources[0].Name);
    }

    [TestMethod]
    public void UpdateTask_ShouldIgnoreSelfInPreviousTasks()
    {
        var Task1 = _taskRepository.Get(t => t.Title == "Task 1");
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
        var Task1 = _taskRepository.Get(t => t.Title == "Task 1");
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
        var Task1 = _taskRepository.Get(t => t.Title == "Task 1");
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
        var Task1 = _taskRepository.Get(t => t.Title == "Task 1");
        var taskDTO = _taskService.GetTask("Generic Project", Task1.Title);

        Assert.AreEqual(1, taskDTO.Resources.Count);
        Assert.AreEqual("Resource 1", taskDTO.Resources[0].Name);
        Assert.AreEqual("Type 1", taskDTO.Resources[0].Type);
        Assert.AreEqual("Description 1", taskDTO.Resources[0].Description);
    }

    [TestMethod]
    public void GetTask_ShouldReturnTaskWithCorrectState()
    {
        var Task1 = _taskRepository.Get(t => t.Title == "Task 1");
        var taskDTO = _taskService.GetTask("Generic Project", Task1.Title);
        Assert.AreEqual(StateDTO.TODO, taskDTO.State);
        var Task2 = _taskRepository.Get(t => t.Title == "Task 2");
        var taskDTO2 = _taskService.GetTask("Generic Project", Task2.Title);
        Assert.AreEqual(StateDTO.DOING, taskDTO2.State);
    }

    [TestMethod]
    public void GetTask_ShouldReturnTaskWithMinimalPreviousTasks()
    {
        var Task1 = _taskRepository.Get(t => t.Title == "Task 1");
        var taskWithDependencies = new Task(
            "Task with Dependencies",
            "Description",
            DateTime.Now,
            3,
            new List<Task> { Task1 },
            new List<Task>(),
            new List<Resource>()
        );
        _taskRepository.Add(taskWithDependencies);
        _projectRepository.AddTask("Generic Project", taskWithDependencies);
        var Task2 = _taskRepository.Get(t => t.Title == "Task with Dependencies");

        var taskDTO = _taskService.GetTask("Generic Project", Task2.Title);

        Assert.AreEqual(1, taskDTO.PreviousTasks.Count);
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
            new List<Resource>
            {
                new("Mapping Resource", "Mapping Type", "Mapping Description")
            }
        );

        task.State = State.DONE;

        _projectRepository.AddTask("Generic Project", task);

        var project = _projectRepository.Get(p => p.Name == "Generic Project");
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
            ExpectedStartDate = new DateTime(2025, 6, 15),
            Duration = 9,
            State = StateDTO.DOING,
            PreviousTasks = new List<TaskDTO>(),
            SameTimeTasks = new List<TaskDTO>(),
            Resources = new List<ResourceDTO>
            {
                resourceDto
            }
        };

        _taskService.AddTask("Generic Project", complexDTO);

        var project = _projectRepository.Get(p => p.Name == "Generic Project");
        var addedTask = project.Tasks.FirstOrDefault(t => t.Title == "Complex Mapping DTO");

        Assert.IsNotNull(addedTask);
        Assert.AreEqual("Complex Mapping DTO", addedTask.Title);
        Assert.AreEqual("Complex mapping description", addedTask.Description);
        Assert.AreEqual(new DateTime(2025, 6, 15), addedTask.ExpectedStartDate);
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

        var project = _projectRepository.Get(p => p.Name == "Generic Project");
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
        Assert.AreEqual("Task X", taskYDTO.PreviousTasks[0].Title);
    }

[TestMethod]
public void UpdateTask_WithComplexRelationships_ShouldCorrectlyMapAllProperties()
{
    var taskA = new Task("Task A", "Description A", DateTime.Now, 1, new List<Task>(), new List<Task>(), new List<Resource>());
    var taskB = new Task("Task B", "Description B", DateTime.Now, 2, new List<Task>(), new List<Task>(), new List<Resource>());
    var taskC = new Task("Task C", "Description C", DateTime.Now, 3, new List<Task>(), new List<Task>(), new List<Resource>());
    
    _taskRepository.Add(taskA);
    _taskRepository.Add(taskB);
    _taskRepository.Add(taskC);
    _context.SaveChanges();  

    _projectRepository.AddTask("Generic Project", taskA);
    _projectRepository.AddTask("Generic Project", taskB);
    _projectRepository.AddTask("Generic Project", taskC);
    
    var addedTaskC = _taskService.GetTask("Generic Project", taskC.Title); 

    Assert.IsNotNull(addedTaskC, "La tarea C no se agregó correctamente");

    _resourceRepository.Add(new Resource { Name = "Complex Update Resource", Type = "Update Type", Description = "Update Resource Description" });
    _context.SaveChanges();
    var resource = _resourceRepository.Get(r => r.Name == "Complex Update Resource");

    var updateDTO = new TaskDTO
    {
        Title = "Updated Task C",
        Description = "Updated Description C",
        ExpectedStartDate = DateTime.Now.AddDays(5),
        Duration = 4,
        PreviousTasks = new List<TaskDTO> { new TaskDTO { Id = taskA.Id } },
        SameTimeTasks = new List<TaskDTO> { new TaskDTO { Id = taskB.Id } },
        Resources = new List<ResourceDTO> { new ResourceDTO { Id = resource.Id } }
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
            new List<Resource>
            {
                new("Special Resource", "Special Type", "Special Description")
            }
        );
        specialTask.State = State.DONE;

        _projectRepository.AddTask("Generic Project", specialTask);

        var project = _projectRepository.Get(p => p.Name == "Generic Project");
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
        var Task1 = _taskRepository.Get(t => t.Title == "Task 1");
        var taskDTO = _taskService.GetTask("Generic Project", Task1.Title);


        Assert.AreEqual(default, taskDTO.Slack);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskException))]
    public void AddTask_ShouldThrowException_WhenTaskStartDateIsBeforeProjectsOne()
    {
        var taskDTO = new TaskDTO
        {
            Title = "Task with invalid start date",
            Description = "Description",
            ExpectedStartDate = DateTime.Now.AddDays(-2),
            Duration = 3,
            Resources = new List<ResourceDTO>()
        };
        
        _taskService.AddTask("Generic Project", taskDTO);
    }
}