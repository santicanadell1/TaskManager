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
    private UserDTO adminUserDTO;
    private UserDTO leaderUserDTO;
    private UserDTO normalUserDTO;
    private Project project;
    private TaskDTO initialTask;
    private readonly IExporter _exporter;

    [TestInitialize]
    public void TestSetUp()
    {
        InMemoryAppContextFactory contextFactory = new InMemoryAppContextFactory();
        _context = contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repositoryManager = new RepositoryManager(_context);
        _cpmService = new CpmService();
        _taskService = new TaskService(_repositoryManager, _cpmService);

        _leaderService = new LeaderPService(_repositoryManager,_exporter );
        _adminService = new AdminPService(_repositoryManager);
        _loginService = new Login(_repositoryManager);
        _userService = new UserService(_repositoryManager);

        adminUserDTO = new UserDTO
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin.user@example.com",
            Password = "AdminPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };

        leaderUserDTO = new UserDTO
        {
            FirstName = "Leader",
            LastName = "User",
            Email = "leader.user@example.com",
            Password = "LeaderPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.ProjectLeader }
        };

        normalUserDTO = new UserDTO
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

        User leaderUser = _repositoryManager.UserRepository.Get(u => u.Email == "leader.user@example.com");
        User adminUser = _repositoryManager.UserRepository.Get(u => u.Email == "admin.user@example.com");

        project = new Project
        {
            Name = "Test Project",
            Description = "Test project description",
            StartDate = DateTime.Now.AddDays(1),
            AdminProject = adminUser,
            ProjectLeader = leaderUser
        };

        _repositoryManager.ProjectRepository.Add(project);

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");
        initialTask = new TaskDTO
        {
            Title = "Initial Task",
            Description = "Initial task for testing",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 3,
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };
        _taskService.AddTask("Test Project", initialTask);

        Project createdProject = _repositoryManager.ProjectRepository.Get(p => p.Name == "Test Project");
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context?.Database.EnsureDeleted();
    }

    [TestMethod]
    public void LeaderPService_ShouldReturnMyProjects_WhenUserIsProjectLeader()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (Project proj in existingProjects)
        {
            _repositoryManager.ProjectRepository.Delete(proj);
        }

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        UserDTO
            leaderUser =
                _userService.GetUser(
                    "leader.user@example.com"); 
        UserDTO
            adminUser = _userService.GetUser(
                "admin.user@example.com"); 

        ProjectDTO project = new ProjectDTO
        {
            Name = "Test Project Direct",
            Description = "Test project description",
            StartDate = DateTime.Now.AddDays(1),
            AdminProyect = adminUser,
        };

        _adminService.CreateProject(project);
        _adminService.SetProjectLeader(project.Name, leaderUser.Email);

        Project verifyProject = _repositoryManager.ProjectRepository.Get(p => p.Name == "Test Project Direct");
        Assert.IsNotNull(verifyProject?.ProjectLeader, "Project leader should not be null after direct creation");

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        List<ProjectDTO> projects = _leaderService.GetAllMyProjects();

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

        _leaderService.UpdateTask("Nonexistent Project", "Some Task", taskDTO);
    }

    [TestMethod]
    public void LeaderPService_ShouldUpdateTask_WhenUserIsProjectLeader()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        TaskDTO updatedTask = new TaskDTO
        {
            Title = "Initial Task",
            Description = "Updated description by leader",
            ExpectedStartDate = DateTime.Now.AddDays(3),
            Duration = 5,
            State = StateDTO.DOING,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.UpdateTask("Test Project", "Initial Task", updatedTask);

        TaskDTO retrievedTask = _leaderService.GetTask("Test Project", "Initial Task");
        Assert.AreEqual("Updated description by leader", retrievedTask.Description);
        Assert.AreEqual(5, retrievedTask.Duration);
        Assert.AreEqual(StateDTO.DOING, retrievedTask.State);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void LeaderPService_ShouldThrowTaskNotFoundException_WhenUpdatingNonexistentTask()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        TaskDTO taskDTO = new TaskDTO
        {
            Title = "Nonexistent Task",
            Description = "This task doesn't exist",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 3,
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.UpdateTask("Test Project", "Nonexistent Task", taskDTO);
    }

    [TestMethod]
    public void LeaderPService_ShouldGetAllTasks_WhenUserIsProjectLeader()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        TaskDTO task2 = new TaskDTO
        {
            Title = "Task 2",
            Description = "Second task",
            ExpectedStartDate = DateTime.Now.AddDays(3),
            Duration = 4,
            State = StateDTO.DOING,
            Resources = new List<ResourceDTO>()
        };

        _taskService.AddTask("Test Project", task2);

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        List<TaskDTO> tasks = _leaderService.GetTasks("Test Project");
        Assert.AreEqual(2, tasks.Count);
        Assert.IsTrue(tasks.Any(t => t.Title == "Initial Task"));
        Assert.IsTrue(tasks.Any(t => t.Title == "Task 2"));
    }

    [TestMethod]
    public void LeaderPService_ShouldGetSpecificTask_WhenUserIsProjectLeader()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        TaskDTO retrievedTask = _leaderService.GetTask("Test Project", "Initial Task");

        Assert.IsNotNull(retrievedTask);
        Assert.AreEqual("Initial Task", retrievedTask.Title);
        Assert.AreEqual("Initial task for testing", retrievedTask.Description);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedLeaderAccessException))]
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

        TaskDTO taskForAnotherProject = new TaskDTO
        {
            Title = "Task in Another Project",
            Description = "Task for unauthorized access test",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 3,
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };
        _taskService.AddTask("Another Project", taskForAnotherProject);

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        TaskDTO taskDTO = new TaskDTO
        {
            Title = "Task in Another Project",
            Description = "Updated description - this should fail",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 3,
            State = StateDTO.DOING,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.UpdateTask("Another Project", "Task in Another Project", taskDTO);
    }

    [TestMethod]
    public void GetProject_ShouldReturnProject_WhenUserIsProjectLeader()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        ProjectDTO projectDTO = _leaderService.GetProject("Test Project");

        Console.WriteLine($"ProjectDTO: {projectDTO != null}");
        Console.WriteLine($"ProjectDTO.Name: {projectDTO?.Name}");
        Console.WriteLine($"ProjectDTO.Description: {projectDTO?.Description}");
        Console.WriteLine($"ProjectDTO.ProjectLeader: {projectDTO?.ProjectLeader != null}");
        Console.WriteLine($"ProjectDTO.ProjectLeader.Email: {projectDTO?.ProjectLeader?.Email}");

        Assert.IsNotNull(projectDTO, "ProjectDTO should not be null");
        Assert.AreEqual("Test Project", projectDTO.Name);
        Assert.AreEqual("Test project description", projectDTO.Description);

        if (projectDTO.ProjectLeader == null)
        {
            Project originalProject = _repositoryManager.ProjectRepository.Get(p => p.Name == "Test Project");
            Console.WriteLine($"Original project leader: {originalProject?.ProjectLeader?.Email}");
        }

        Assert.IsNotNull(projectDTO.ProjectLeader, "ProjectLeader should not be null");
        Assert.AreEqual("leader.user@example.com", projectDTO.ProjectLeader.Email);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedLeaderAccessException))]
    public void LeaderPService_ShouldThrowUnauthorizedAccessException_WhenUserIsNotProjectLeader()
    {
        _loginService.LoginUser("normal.user@example.com", "Password123@");

        TaskDTO taskDTO = new TaskDTO
        {
            Title = "Initial Task",
            Description = "This should fail",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 3,
            State = StateDTO.DOING,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.UpdateTask("Test Project", "Initial Task", taskDTO);
    }


    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void GetProject_ShouldThrowProjectNotFoundException_WhenProjectDoesNotExist()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        _leaderService.GetProject("Nonexistent Project");
    }

    [TestMethod]
    public void LeaderPService_ShouldRemoveTempAdminProjectRole_AfterAssigningMembers()
    {
        List<UserDTO> membersToAdd = new List<UserDTO>
        {
            _userService.GetUser(normalUserDTO.Email)
        };

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");
        _leaderService.AssignMembersToProject("Test Project", membersToAdd);
    }

    [TestMethod]
    public void GetAllMembersOfAProject_ShouldGetMembers_WhenUserIsAdmin()
    {
        List<UserDTO> membersToAdd = new List<UserDTO>
        {
            _userService.GetUser(normalUserDTO.Email)
        };

        _loginService.LoginUser(leaderUserDTO.Email, leaderUserDTO.Password);

        _leaderService.AssignMembersToProject(project.Name, membersToAdd);

        List<UserDTO> members = _leaderService.GetAllMembersOfAProject(project.Name);

        Assert.IsTrue(members.Exists(m => m.Email == normalUserDTO.Email));
    }

    [TestMethod]
    public void RemoveMember_ShouldRemoveMember_WhenUserIsAdmin()
    {
        List<UserDTO> membersToAdd = new List<UserDTO>
        {
            _userService.GetUser(normalUserDTO.Email)
        };

        _loginService.LoginUser(leaderUserDTO.Email, leaderUserDTO.Password);

        _leaderService.AssignMembersToProject(project.Name, membersToAdd);

        project = _repositoryManager.ProjectRepository.Get(p => p.Name == project.Name);
        Assert.IsNotNull(project);
        Assert.IsTrue(project.Members.Exists(m => m.Email == normalUserDTO.Email));

        _leaderService.RemoveMemberFromProject(project.Name, normalUserDTO.Email);

        project = _repositoryManager.ProjectRepository.Get(p => p.Name == project.Name);
        Assert.IsFalse(project.Members.Exists(m => m.Email == normalUserDTO.Email));
    }

    [TestMethod]
    public void GetAllTaskForAMemberInAProject_ShouldReturnTasks_WhenUserIsAdmin()
    {
        List<UserDTO> membersToAdd = new List<UserDTO>
        {
            _userService.GetUser(normalUserDTO.Email)
        };

        _loginService.LoginUser(leaderUserDTO.Email, leaderUserDTO.Password);

        _leaderService.AssignMembersToProject(project.Name, membersToAdd);

        TaskDTO task1 = new TaskDTO
        {
            Title = "Task 1",
            Description = "Task 1 description",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 5,
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };

        TaskDTO task2 = new TaskDTO
        {
            Title = "Task 2",
            Description = "Task 2 description",
            ExpectedStartDate = DateTime.Now.AddDays(3),
            Duration = 7,
            State = StateDTO.DOING,
            Resources = new List<ResourceDTO>()
        };

        _loginService.LoginUser(adminUserDTO.Email, adminUserDTO.Password);

        _taskService.AddTask(project.Name, task1);
        _taskService.AddTask(project.Name, task2);
        _adminService.AddTaskToMember(project.Name, normalUserDTO.Email, task1.Title);
        _adminService.AddTaskToMember(project.Name, normalUserDTO.Email, task2.Title);

        _loginService.LoginUser(leaderUserDTO.Email, leaderUserDTO.Password);

        List<TaskDTO> tasksForMember = _leaderService.GetAllTaskForAMemberInAProject(project.Name, normalUserDTO.Email);

        Assert.IsNotNull(tasksForMember);
        Assert.AreEqual(2, tasksForMember.Count);
        Assert.IsTrue(tasksForMember.Any(t => t.Title == "Task 1"));
        Assert.IsTrue(tasksForMember.Any(t => t.Title == "Task 2"));
    }

    [TestMethod]
    public void AddTaskToMember_ShouldAddTask_WhenUserIsAdmin()
    {
        List<UserDTO> membersToAdd = new List<UserDTO>
        {
            _userService.GetUser(normalUserDTO.Email)
        };

        _loginService.LoginUser(leaderUserDTO.Email, leaderUserDTO.Password);
        _leaderService.AssignMembersToProject(project.Name, membersToAdd);

        _loginService.LoginUser(adminUserDTO.Email, adminUserDTO.Password);

        _leaderService.AddTaskToMember(project.Name, normalUserDTO.Email, initialTask.Title);

        List<TaskDTO> tasksForMember = _leaderService.GetAllTaskForAMemberInAProject(project.Name, normalUserDTO.Email);
        Assert.IsNotNull(tasksForMember);
        Assert.IsTrue(tasksForMember.Any(t => t.Title == initialTask.Title),
            "The task should be added to the member's task list.");
    }


    [TestMethod]
    public void RemoveTaskFromMember_ShouldRemoveTask_WhenUserIsAdmin()
    {
        List<UserDTO> membersToAdd = new List<UserDTO>
        {
            _userService.GetUser(normalUserDTO.Email)
        };

        _loginService.LoginUser(leaderUserDTO.Email, leaderUserDTO.Password);
        _leaderService.AssignMembersToProject(project.Name, membersToAdd);
        _leaderService.AddTaskToMember(project.Name, normalUserDTO.Email, initialTask.Title);

        List<TaskDTO> tasksBeforeRemoval =
            _leaderService.GetAllTaskForAMemberInAProject(project.Name, normalUserDTO.Email);
        Assert.IsTrue(tasksBeforeRemoval.Any(t => t.Title == initialTask.Title), "Task should exist before removal");

        _leaderService.RemoveTaskFromMember(project.Name, normalUserDTO.Email, initialTask.Title);

        List<TaskDTO> tasksAfterRemoval =
            _leaderService.GetAllTaskForAMemberInAProject(project.Name, normalUserDTO.Email);
        Assert.IsFalse(tasksAfterRemoval.Any(t => t.Title == initialTask.Title),
            "Task should be removed from the member's task list.");
    }

 [TestMethod]
public void ExportProjects_CSV_ShouldReturnCorrectFormat_WhenUserIsProjectLeader()
{
    _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");
    
    var csvExporter = new CSVExporter(_repositoryManager);
    var leaderServiceWithCsv = new LeaderPService(_repositoryManager, csvExporter);
    
    _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");
    
    TaskDTO task2 = new TaskDTO
    {
        Title = "Zeta Task",
        Description = "Task with Z to test ordering",
        ExpectedStartDate = DateTime.Now.AddDays(4),
        Duration = 2,
        State = StateDTO.DOING,
        Resources = new List<ResourceDTO>() 
    };
    
    TaskDTO task3 = new TaskDTO
    {
        Title = "Alpha Task",
        Description = "Task with A to test ordering",
        ExpectedStartDate = DateTime.Now.AddDays(5),
        Duration = 6,
        State = StateDTO.TODO,
        Resources = new List<ResourceDTO>() 
    };
    
    _taskService.AddTask("Test Project", task2);
    _taskService.AddTask("Test Project", task3);
    
    _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");
    
    string csvResult = leaderServiceWithCsv.ExportProjects();
    
    Assert.IsNotNull(csvResult);
    Assert.IsTrue(csvResult.Contains("Proyecto,Fecha de Inicio,Tarea,Fecha de Inicio,Duración,Crítico,Recursos"));
    Assert.IsTrue(csvResult.Contains("Test Project"));
    Assert.IsTrue(csvResult.Contains("Zeta Task"));
    Assert.IsTrue(csvResult.Contains("Alpha Task"));
    Assert.IsTrue(csvResult.Contains("Initial Task"));
    
    string[] lines = csvResult.Split('\n');
    
    int zetaIndex = Array.FindIndex(lines, line => line.Contains("Zeta Task"));
    int initialIndex = Array.FindIndex(lines, line => line.Contains("Initial Task"));
    int alphaIndex = Array.FindIndex(lines, line => line.Contains("Alpha Task"));
    
    Assert.IsTrue(zetaIndex < initialIndex, "Zeta Task should appear before Initial Task");
    Assert.IsTrue(initialIndex < alphaIndex, "Initial Task should appear before Alpha Task");
    
    Console.WriteLine("=== RESULTADO CSV ===");
    Console.WriteLine(csvResult);
}

[TestMethod]
public void ExportProjects_JSON_ShouldReturnCorrectFormat_WhenUserIsProjectLeader()
{
    _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");
    
    var jsonExporter = new JSONExporter(_repositoryManager);
    var leaderServiceWithJson = new LeaderPService(_repositoryManager, jsonExporter);
    
    _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");
    
    TaskDTO taskWithoutResources = new TaskDTO
    {
        Title = "Task With Resources", 
        Description = "Task to test resource export",
        ExpectedStartDate = DateTime.Now.AddDays(3),
        Duration = 4,
        State = StateDTO.DOING,
        Resources = new List<ResourceDTO>() 
    };
    
    _taskService.AddTask("Test Project", taskWithoutResources);
    
    _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");
    
    string jsonResult = leaderServiceWithJson.ExportProjects();
    
    Assert.IsNotNull(jsonResult);
    Assert.IsTrue(jsonResult.Contains("\"Project\""));
    Assert.IsTrue(jsonResult.Contains("\"StartDate\""));
    Assert.IsTrue(jsonResult.Contains("\"Tasks\""));
    Assert.IsTrue(jsonResult.Contains("\"Task\""));
    Assert.IsTrue(jsonResult.Contains("\"Duration\""));
    Assert.IsTrue(jsonResult.Contains("\"IsCritical\""));
    Assert.IsTrue(jsonResult.Contains("\"Resources\""));
    Assert.IsTrue(jsonResult.Contains("Test Project"));
    Assert.IsTrue(jsonResult.Contains("Task With Resources"));
    
    
    Console.WriteLine("=== RESULTADO JSON ===");
    Console.WriteLine(jsonResult);
}

[TestMethod]
public void ExportProjects_ShouldReturnEmptyExport_WhenNoProjectsExist()
{
    List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
    foreach (Project proj in existingProjects)
    {
        _repositoryManager.ProjectRepository.Delete(proj);
    }
    
    _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");
    
    var csvExporter = new CSVExporter(_repositoryManager);
    var leaderServiceWithCsv = new LeaderPService(_repositoryManager, csvExporter);
    
    string csvResult = leaderServiceWithCsv.ExportProjects();
    
    // Assert
    Assert.IsNotNull(csvResult);
    Assert.IsTrue(csvResult.Contains("Proyecto,Fecha de Inicio,Tarea,Fecha de Inicio,Duración,Crítico,Recursos"));
    
    string[] lines = csvResult.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    Assert.AreEqual(1, lines.Length, "Should only contain header when no projects exist");
    
    Console.WriteLine("=== RESULTADO SIN PROYECTOS ===");
    Console.WriteLine(csvResult);
}




}