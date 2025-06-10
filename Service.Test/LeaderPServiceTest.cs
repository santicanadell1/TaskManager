using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Exceptions.LeaderPServiceException;
using Service.Models;
using Task = Domain.Task;

namespace Service.Test;

[TestClass]
public class LeaderPService_Test
{
    private LeaderPService _leaderService;
    private AdminPService _adminService;
    private TaskService _taskService;
    private AppDbContext _context;
    private Login _loginService;
    private UserService _userService;
    private IRepositoryManager _repositoryManager;
    private CpmService _cpmService;

[TestInitialize]
public void TestSetUp()
{
    InMemoryAppContextFactory contextFactory = new InMemoryAppContextFactory();
    _context = contextFactory.CreateDbContext();

    _context.Database.EnsureDeleted();
    _context.Database.EnsureCreated();

    _repositoryManager = new RepositoryManager(_context);
    _cpmService = new CpmService();

    _leaderService = new LeaderPService(_repositoryManager);
    _adminService = new AdminPService(_repositoryManager);
    _taskService = new TaskService(_repositoryManager, _cpmService);
    _loginService = new Login(_repositoryManager);
    _userService = new UserService(_repositoryManager);

    UserDTO adminUserDTO = new UserDTO
    {
        FirstName = "Admin",
        LastName = "User",
        Email = "admin.user@example.com",
        Password = "AdminPassword123@",
        Birthday = DateTime.Parse("1990-01-01"),
        Roles = new List<RolDTO> { RolDTO.AdminProject }
    };

    UserDTO leaderUserDTO = new UserDTO
    {
        FirstName = "Leader",
        LastName = "User",
        Email = "leader.user@example.com",
        Password = "LeaderPassword123@",
        Birthday = DateTime.Parse("1990-01-01"),
        Roles = new List<RolDTO> { RolDTO.ProjectLeader }
    };

    UserDTO normalUserDTO = new UserDTO
    {
        FirstName = "Normal",
        LastName = "User",
        Email = "normal.user@example.com",
        Password = "Password123@",
        Birthday = DateTime.Parse("1990-01-01"),
        Roles = new List<RolDTO> { RolDTO.ProjectMember }
    };

    _userService.AddUser(adminUserDTO);
    _userService.AddUser(leaderUserDTO);
    _userService.AddUser(normalUserDTO);

    
    var leaderUser = _repositoryManager.UserRepository.Get(u => u.Email == "leader.user@example.com");
    var adminUser = _repositoryManager.UserRepository.Get(u => u.Email == "admin.user@example.com");

    var project = new Project
    {
        Name = "Test Project",
        Description = "Test project description",
        StartDate = DateTime.Now.AddDays(1),
        AdminProject = adminUser,
        ProjectLeader = leaderUser  
    };

    _repositoryManager.ProjectRepository.Add(project);

    var createdProject = _repositoryManager.ProjectRepository.Get(p => p.Name == "Test Project");
    Console.WriteLine($"TestSetUp verification - Project created with Leader: {createdProject?.ProjectLeader?.Email}");
}

    [TestCleanup]
    public void CleanUp()
    {
        _context?.Database.EnsureDeleted();
    }
    
    [TestMethod]
    public void AddTask_ShouldAddTaskSuccessfully_WhenUserIsProjectLeader()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");
    
        TaskDTO taskDTO = new TaskDTO
        {
            Title = "Test Task",
            Description = "Test task description",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 5,
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.AddTask("Test Project", taskDTO);
    
    }
    
    [TestMethod]
    public void LeaderPService_ShouldReturnMyProjects_WhenUserIsProjectLeader_Workaround()
    {
        var existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (var proj in existingProjects)
        {
            _repositoryManager.ProjectRepository.Delete(proj);
        }
        
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var leaderUser = _repositoryManager.UserRepository.Get(u => u.Email == "leader.user@example.com");
        var adminUser = _repositoryManager.UserRepository.Get(u => u.Email == "admin.user@example.com");

        var project = new Project
        {
            Name = "Test Project Direct",
            Description = "Test project description",
            StartDate = DateTime.Now.AddDays(1),
            AdminProject = adminUser,
            ProjectLeader = leaderUser  
        };

        _repositoryManager.ProjectRepository.Add(project);
      

        var verifyProject = _repositoryManager.ProjectRepository.Get(p => p.Name == "Test Project Direct");
        Console.WriteLine($"Verification - Project Leader: {verifyProject?.ProjectLeader?.Email}");
        Assert.IsNotNull(verifyProject?.ProjectLeader, "Project leader should not be null after direct creation");

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        List<ProjectDTO> projects = _leaderService.GetMyProjects();

        Assert.AreEqual(1, projects.Count);
        Assert.AreEqual("Test Project Direct", projects[0].Name);
    }
    
    
    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void LeaderPService_ShouldThrowProjectNotFoundException_WhenProjectDoesNotExist()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        TaskDTO taskDTO = new TaskDTO
        {
            Title = "Test Task",
            Description = "Test task description",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 5,
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.AddTask("Nonexistent Project", taskDTO);
    }

    [TestMethod]
    public void LeaderPService_ShouldUpdateTask_WhenUserIsProjectLeader()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        TaskDTO originalTask = new TaskDTO
        {
            Title = "Original Task",
            Description = "Original description",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 3,
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.AddTask("Test Project", originalTask);

        TaskDTO updatedTask = new TaskDTO
        {
            Title = "Original Task", 
            Description = "Updated description",
            ExpectedStartDate = DateTime.Now.AddDays(3),
            Duration = 5,
            State = StateDTO.DOING,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.UpdateTask("Test Project", "Original Task", updatedTask);

        TaskDTO retrievedTask = _leaderService.GetTask("Test Project", "Original Task");
        Assert.AreEqual("Updated description", retrievedTask.Description);
        Assert.AreEqual(5, retrievedTask.Duration);
        Assert.AreEqual(StateDTO.DOING, retrievedTask.State);
    }
    
    [TestMethod]
    public void LeaderPService_ShouldDeleteTask_WhenUserIsProjectLeader()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        TaskDTO taskDTO = new TaskDTO
        {
            Title = "Task To Delete",
            Description = "This task will be deleted",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 3,
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.AddTask("Test Project", taskDTO);

        List<TaskDTO> tasksBeforeDelete = _leaderService.GetTasks("Test Project");
        Assert.AreEqual(1, tasksBeforeDelete.Count);

        _leaderService.DeleteTask("Test Project", "Task To Delete");

        List<TaskDTO> tasksAfterDelete = _leaderService.GetTasks("Test Project");
        Assert.AreEqual(0, tasksAfterDelete.Count);
    }
    
    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void LeaderPService_ShouldThrowTaskNotFoundException_WhenTaskDoesNotExist()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        _leaderService.DeleteTask("Test Project", "Nonexistent Task");
    }
  
    [TestMethod]
    public void LeaderPService_ShouldGetAllTasks_WhenUserIsProjectLeader()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        TaskDTO task1 = new TaskDTO
        {
            Title = "Task 1",
            Description = "First task",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 3,
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };

        TaskDTO task2 = new TaskDTO
        {
            Title = "Task 2",
            Description = "Second task",
            ExpectedStartDate = DateTime.Now.AddDays(3),
            Duration = 4,
            State = StateDTO.DOING,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.AddTask("Test Project", task1);
        _leaderService.AddTask("Test Project", task2);

        List<TaskDTO> tasks = _leaderService.GetTasks("Test Project");
        Assert.AreEqual(2, tasks.Count);
        Assert.IsTrue(tasks.Any(t => t.Title == "Task 1"));
        Assert.IsTrue(tasks.Any(t => t.Title == "Task 2"));
    }
    
    [TestMethod]
    public void LeaderPService_ShouldGetCriticalPath_WhenUserIsProjectLeader()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        TaskDTO task = new TaskDTO
        {
            Title = "Critical Task",
            Description = "Task for critical path test",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 5,
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.AddTask("Test Project", task);

        CpmResultDTO criticalPath = _leaderService.GetCriticalPath("Test Project");
        Assert.IsNotNull(criticalPath);
        Assert.IsTrue(criticalPath.ProjectDuration >= 0);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void LeaderPService_ShouldThrowUnauthorizedAccessException_WhenUserIsNotLeaderOfSpecificProject()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");
        
        UserDTO anotherLeader = new UserDTO
        {
            FirstName = "Another",
            LastName = "Leader",
            Email = "another.leader@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.ProjectLeader }
        };
        
        _userService.AddUser(anotherLeader);

        ProjectDTO anotherProject = new ProjectDTO
        {
            Name = "Another Project",
            Description = "Another project description",
            StartDate = DateTime.Now.AddDays(1),
            AdminProyect = _userService.GetUser("admin.user@example.com"),
            ProjectLeader = anotherLeader,
            Members = new List<UserDTO>()
        };

        _adminService.CreateProject(anotherProject);

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        TaskDTO taskDTO = new TaskDTO
        {
            Title = "Unauthorized Task",
            Description = "This should fail",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 3,
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.AddTask("Another Project", taskDTO);
    }
    

}