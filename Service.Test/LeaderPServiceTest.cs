using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Newtonsoft.Json;
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

        _leaderService = new LeaderPService(_repositoryManager, _exporter);
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
    List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
    foreach (Project proj in existingProjects)
    {
        _repositoryManager.ProjectRepository.Delete(proj);
    }

    _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

    DateTime baseDate = DateTime.Now.AddDays(10);

    ProjectDTO project1 = new ProjectDTO
    {
        Name = "Proyecto A",
        Description = "Primer proyecto",
        StartDate = baseDate,
        AdminProyect = _userService.GetUser("admin.user@example.com")
    };

    ProjectDTO project2 = new ProjectDTO
    {
        Name = "Proyecto B",
        Description = "Segundo proyecto",
        StartDate = baseDate.AddDays(10),
        AdminProyect = _userService.GetUser("admin.user@example.com")
    };

    _adminService.CreateProject(project1);
    _adminService.CreateProject(project2);

    _adminService.SetProjectLeader("Proyecto A", "leader.user@example.com");
    _adminService.SetProjectLeader("Proyecto B", "leader.user@example.com");


    TaskDTO tarea1_A = new TaskDTO
    {
        Title = "Tarea Z",
        Description = "Tercera tarea del proyecto A",
        ExpectedStartDate = baseDate.AddDays(1),
        StartDate = baseDate.AddDays(1),
        Duration = 5,
        State = StateDTO.TODO,
        IsCritical = false,
        Resources = new List<ResourceDTO>()
    };

    TaskDTO tarea2_A = new TaskDTO
    {
        Title = "Alpha",
        Description = "Cuarta tarea del proyecto A",
        ExpectedStartDate = baseDate.AddDays(3),
        StartDate = baseDate.AddDays(3),
        Duration = 3,
        State = StateDTO.DOING,
        IsCritical = true,
        Resources = new List<ResourceDTO>()
    };

    TaskDTO tarea3_A = new TaskDTO
    {
        Title = "Zebra",
        Description = "Primera tarea del proyecto A",
        ExpectedStartDate = baseDate.AddDays(2),
        StartDate = baseDate.AddDays(2),
        Duration = 4,
        State = StateDTO.TODO,
        IsCritical = true,
        Resources = new List<ResourceDTO>()
    };

    TaskDTO tarea4_A = new TaskDTO
    {
        Title = "Medio",
        Description = "Segunda tarea del proyecto A",
        ExpectedStartDate = baseDate.AddDays(4),
        StartDate = baseDate.AddDays(4),
        Duration = 2,
        State = StateDTO.DOING,
        IsCritical = false,
        Resources = new List<ResourceDTO>()
    };

    TaskDTO tarea1_B = new TaskDTO
    {
        Title = "T1",
        Description = "Tarea del proyecto B",
        ExpectedStartDate = baseDate.AddDays(11),
        StartDate = baseDate.AddDays(11),
        Duration = 2,
        State = StateDTO.TODO,
        IsCritical = true,
        Resources = new List<ResourceDTO>()
    };

    _taskService.AddTask("Proyecto A", tarea1_A);
    _taskService.AddTask("Proyecto A", tarea2_A);
    _taskService.AddTask("Proyecto A", tarea3_A);
    _taskService.AddTask("Proyecto A", tarea4_A);
    _taskService.AddTask("Proyecto B", tarea1_B);

    _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

    var csvExporter = new CSVExporter(_repositoryManager);
    var leaderServiceWithCsv = new LeaderPService(_repositoryManager, csvExporter);

    string csvResult = leaderServiceWithCsv.ExportProjects();
    Assert.IsNotNull(csvResult);

    string expectedDateA    = baseDate.ToString("dd/MM/yyyy");
    string expectedDateB    = baseDate.AddDays(10).ToString("dd/MM/yyyy");
    string expectedDateT1   = baseDate.AddDays(1).ToString("dd/MM/yyyy");
    string expectedDateT2   = baseDate.AddDays(3).ToString("dd/MM/yyyy");
    string expectedDateT3   = baseDate.AddDays(2).ToString("dd/MM/yyyy");
    string expectedDateT4   = baseDate.AddDays(4).ToString("dd/MM/yyyy");
    string expectedDateT5   = baseDate.AddDays(11).ToString("dd/MM/yyyy");

    var lines = csvResult
        .Split(new[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
    
    Assert.AreEqual($"Proyecto A,{expectedDateA}", lines[0], "La primera línea debe ser 'Proyecto A,fecha'");
    Assert.AreEqual($"Zebra,{expectedDateT3},S",    lines[1], "Primera tarea debe ser Zebra con crítico S");
    Assert.AreEqual($"Tarea Z,{expectedDateT1},S", lines[2], "Segunda tarea debe ser Tarea Z con crítico S");
    Assert.AreEqual($"Medio,{expectedDateT4},S",   lines[3], "Tercera tarea debe ser Medio con crítico S");
    Assert.AreEqual($"Alpha,{expectedDateT2},S",   lines[4], "Cuarta tarea debe ser Alpha con crítico S");
    int idxB = Array.FindIndex(lines, l => l.StartsWith("Proyecto B,"));
    Assert.IsTrue(idxB > 0, "Debe existir una línea que empiece con 'Proyecto B,'");
    Assert.AreEqual($"Proyecto B,{expectedDateB}", lines[idxB], "Línea de Proyecto B incorrecta");
    Assert.AreEqual($"T1,{expectedDateT5},S", lines[idxB + 1], "Tarea T1 de Proyecto B incorrecta");
}


    [TestMethod]
    public void ExportProjects_JSON_ShouldReturnCorrectFormat_WhenUserIsProjectLeader()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (Project proj in existingProjects)
        {
            _repositoryManager.ProjectRepository.Delete(proj);
        }

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        DateTime baseDate = DateTime.Now.AddDays(10);

        ProjectDTO project1 = new ProjectDTO
        {
            Name = "Proyecto A",
            Description = "Primer proyecto",
            StartDate = baseDate,
            AdminProyect = _userService.GetUser("admin.user@example.com")
        };

        ProjectDTO project2 = new ProjectDTO
        {
            Name = "Proyecto B",
            Description = "Segundo proyecto",
            StartDate = baseDate.AddDays(10),
            AdminProyect = _userService.GetUser("admin.user@example.com")
        };

        _adminService.CreateProject(project1);
        _adminService.CreateProject(project2);

        _adminService.SetProjectLeader("Proyecto A", "leader.user@example.com");
        _adminService.SetProjectLeader("Proyecto B", "leader.user@example.com");

        TaskDTO tarea1_A = new TaskDTO
        {
            Title = "Tarea Z",
            Description = "Tercera tarea del proyecto A",
            ExpectedStartDate = baseDate.AddDays(1),
            StartDate = baseDate.AddDays(1),
            Duration = 5,
            State = StateDTO.TODO,
            IsCritical = false,
            Resources = new List<ResourceDTO>()
        };

        TaskDTO tarea2_A = new TaskDTO
        {
            Title = "Alpha",
            Description = "Cuarta tarea del proyecto A",
            ExpectedStartDate = baseDate.AddDays(3),
            StartDate = baseDate.AddDays(3),
            Duration = 3,
            State = StateDTO.DOING,
            IsCritical = true,
            Resources = new List<ResourceDTO>()
        };

        TaskDTO tarea3_A = new TaskDTO
        {
            Title = "Zebra",
            Description = "Primera tarea del proyecto A",
            ExpectedStartDate = baseDate.AddDays(2),
            StartDate = baseDate.AddDays(2),
            Duration = 4,
            State = StateDTO.TODO,
            IsCritical = true,
            Resources = new List<ResourceDTO>()
        };

        TaskDTO tarea4_A = new TaskDTO
        {
            Title = "Medio",
            Description = "Segunda tarea del proyecto A",
            ExpectedStartDate = baseDate.AddDays(4),
            StartDate = baseDate.AddDays(4),
            Duration = 2,
            State = StateDTO.DOING,
            IsCritical = false,
            Resources = new List<ResourceDTO>()
        };


        TaskDTO tarea1_B = new TaskDTO
        {
            Title = "T1",
            Description = "Tarea del proyecto B",
            ExpectedStartDate = baseDate.AddDays(11),
            StartDate = baseDate.AddDays(11),
            Duration = 2,
            State = StateDTO.TODO,
            IsCritical = true,
            Resources = new List<ResourceDTO>()
        };

        _taskService.AddTask("Proyecto A", tarea1_A);
        _taskService.AddTask("Proyecto A", tarea2_A);
        _taskService.AddTask("Proyecto A", tarea3_A);
        _taskService.AddTask("Proyecto A", tarea4_A);
        _taskService.AddTask("Proyecto B", tarea1_B);

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var jsonExporter = new JSONExporter(_repositoryManager);
        var leaderServiceWithJson = new LeaderPService(_repositoryManager, jsonExporter);

        string jsonResult = leaderServiceWithJson.ExportProjects();

        Console.WriteLine("=== RESULTADO JSON ===");
        Console.WriteLine(jsonResult);


        Assert.IsNotNull(jsonResult);
        Assert.IsTrue(jsonResult.Contains("\"Project\""));
        Assert.IsTrue(jsonResult.Contains("\"StartDate\""));
        Assert.IsTrue(jsonResult.Contains("\"Tasks\""));
        Assert.IsTrue(jsonResult.Contains("\"Task\""));
        Assert.IsTrue(jsonResult.Contains("\"Duration\""));
        Assert.IsTrue(jsonResult.Contains("\"IsCritical\""));
        Assert.IsTrue(jsonResult.Contains("\"Resources\""));

        Assert.IsTrue(jsonResult.Contains("\"Proyecto A\""));
        Assert.IsTrue(jsonResult.Contains("\"Proyecto B\""));

        Assert.IsTrue(jsonResult.Contains("\"Zebra\""));
        Assert.IsTrue(jsonResult.Contains("\"Tarea Z\""));
        Assert.IsTrue(jsonResult.Contains("\"Medio\""));
        Assert.IsTrue(jsonResult.Contains("\"Alpha\""));
        Assert.IsTrue(jsonResult.Contains("\"T1\""));

        string expectedDate1 = baseDate.ToString("dd/MM/yyyy");
        string expectedDate2 = baseDate.AddDays(10).ToString("dd/MM/yyyy");
        string expectedTaskDate1 = baseDate.AddDays(1).ToString("dd/MM/yyyy");
        string expectedTaskDate2 = baseDate.AddDays(3).ToString("dd/MM/yyyy");
        string expectedTaskDate3 = baseDate.AddDays(2).ToString("dd/MM/yyyy");
        string expectedTaskDate4 = baseDate.AddDays(4).ToString("dd/MM/yyyy");
        string expectedTaskDate5 = baseDate.AddDays(11).ToString("dd/MM/yyyy");
        Console.WriteLine($"Fecha esperada proyecto A: {expectedDate1}");
        Console.WriteLine($"Fecha esperada proyecto B: {expectedDate2}");
        Console.WriteLine($"Fecha esperada Tarea Z: {expectedTaskDate1}");
        Console.WriteLine($"Fecha esperada Alpha: {expectedTaskDate2}");
        Console.WriteLine($"Fecha esperada Zebra: {expectedTaskDate3}");
        Console.WriteLine($"Fecha esperada Medio: {expectedTaskDate4}");
        Console.WriteLine($"Fecha esperada T1: {expectedTaskDate5}");

        Assert.IsTrue(jsonResult.Contains($"\"{expectedDate1}\""), $"Debe contener la fecha {expectedDate1}");
        Assert.IsTrue(jsonResult.Contains($"\"{expectedDate2}\""), $"Debe contener la fecha {expectedDate2}");
        Assert.IsTrue(jsonResult.Contains($"\"{expectedTaskDate1}\""),
            $"Debe contener la fecha de Tarea Z {expectedTaskDate1}");
        Assert.IsTrue(jsonResult.Contains($"\"{expectedTaskDate2}\""),
            $"Debe contener la fecha de Alpha {expectedTaskDate2}");
        Assert.IsTrue(jsonResult.Contains($"\"{expectedTaskDate3}\""),
            $"Debe contener la fecha de Zebra {expectedTaskDate3}");
        Assert.IsTrue(jsonResult.Contains($"\"{expectedTaskDate4}\""),
            $"Debe contener la fecha de Medio {expectedTaskDate4}");
        Assert.IsTrue(jsonResult.Contains($"\"{expectedTaskDate5}\""),
            $"Debe contener la fecha de T1 {expectedTaskDate5}");

        Assert.IsTrue(jsonResult.Contains("\"IsCritical\": \"N\""));
        Assert.IsTrue(jsonResult.Contains("\"IsCritical\": \"S\""));

        Assert.IsTrue(jsonResult.Contains("\"Duration\": 5"));
        Assert.IsTrue(jsonResult.Contains("\"Duration\": 3"));
        Assert.IsTrue(jsonResult.Contains("\"Duration\": 4"));
        Assert.IsTrue(jsonResult.Contains("\"Duration\": 2"));

        Assert.IsTrue(jsonResult.Contains("\"Resources\": []"));

        var projects = JsonConvert.DeserializeObject<List<dynamic>>(jsonResult);
        Assert.AreEqual(2, projects.Count, "Debe haber exactamente 2 proyectos");

        Assert.AreEqual("Proyecto A", (string)projects[0].Project);
        Assert.AreEqual("Proyecto B", (string)projects[1].Project);

        var tasksByProjectA = projects[0].Tasks;
        Assert.AreEqual(4, tasksByProjectA.Count, "Proyecto A debe tener 4 tareas");

        Assert.AreEqual("Zebra", (string)tasksByProjectA[0].Task);
        Assert.AreEqual("Tarea Z", (string)tasksByProjectA[1].Task);
        Assert.AreEqual("Medio", (string)tasksByProjectA[2].Task);
        Assert.AreEqual("Alpha", (string)tasksByProjectA[3].Task);

        var tasksByProjectB = projects[1].Tasks;
        Assert.AreEqual(1, tasksByProjectB.Count, "Proyecto B debe tener 1 tarea");
        Assert.AreEqual("T1", (string)tasksByProjectB[0].Task);
    }

    [TestMethod]
    public void ExportProjects_JSON_ShouldReturnEmptyArray_WhenNoProjectsExist()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (Project proj in existingProjects)
        {
            _repositoryManager.ProjectRepository.Delete(proj);
        }

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var jsonExporter = new JSONExporter(_repositoryManager);
        var leaderServiceWithJson = new LeaderPService(_repositoryManager, jsonExporter);

        string jsonResult = leaderServiceWithJson.ExportProjects();

        Assert.IsNotNull(jsonResult, "El resultado no debe ser null");

        var deserializedResult = JsonConvert.DeserializeObject<List<object>>(jsonResult);
        Assert.IsNotNull(deserializedResult, "El resultado deserializado no debe ser null");
        Assert.AreEqual(0, deserializedResult.Count, "El array debe estar vacío cuando no hay proyectos");

        string normalizedJson = jsonResult.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        Assert.AreEqual("[]", normalizedJson, "El JSON debe ser un array vacío");

        Console.WriteLine("=== RESULTADO JSON SIN PROYECTOS ===");
        Console.WriteLine($"Contenido: '{jsonResult}'");
    }

    [TestMethod]
    public void ExportProjects_CSV_ShouldReturnEmpty_WhenNoProjectsExist()
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
        Assert.IsNotNull(csvResult, "El resultado no debe ser null");
        string trimmed = csvResult.Trim();
        Assert.IsTrue(string.IsNullOrEmpty(trimmed),
            "Debe devolver cadena vacía cuando no hay proyectos");
        string[] lines = csvResult
            .Split(new[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(0, lines.Length,
            "No debe haber líneas cuando no hay proyectos");

        Console.WriteLine("=== RESULTADO SIN PROYECTOS ===");
        Console.WriteLine($"Número de líneas no vacías: {lines.Length}");
        Console.WriteLine($"Contenido (raw): '{csvResult}'");
    }
}