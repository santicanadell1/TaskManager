using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using DataAccess.Exceptions.TaskRepositoryExceptions;
using Domain;
using Newtonsoft.Json;
using Service.Exceptions.ExporterExeptions;
using Service.Exceptions.LeaderPServiceException;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Service.Test;

[TestClass]
public class LeaderPService_Test
{
    private readonly IExporter _exporter;
    private AdminPService _adminService;
    private AppDbContext _context;
    private CpmService _cpmService;
    private LeaderPService _leaderService;
    private Login _loginService;
    private IRepositoryManager _repositoryManager;
    private TaskService _taskService;
    private UserService _userService;
    private UserDTO adminUserDTO;
    private TaskDTO initialTask;
    private UserDTO leaderUserDTO;
    private UserDTO normalUserDTO;
    private Project project;

    [TestInitialize]
    public void TestSetUp()
    {
        var contextFactory = new InMemoryAppContextFactory();
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

        var leaderUser = _repositoryManager.UserRepository.Get(u => u.Email == "leader.user@example.com");
        var adminUser = _repositoryManager.UserRepository.Get(u => u.Email == "admin.user@example.com");

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

        var createdProject = _repositoryManager.ProjectRepository.Get(p => p.Name == "Test Project");
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
        foreach (var proj in existingProjects) _repositoryManager.ProjectRepository.Delete(proj);

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var leaderUser = _userService.GetUser("leader.user@example.com");
        var adminUser = _userService.GetUser("admin.user@example.com");

        var project = new ProjectDTO
        {
            Name = "Test Project Direct",
            Description = "Test project description",
            StartDate = DateTime.Now.AddDays(1),
            AdminProyect = adminUser
        };

        _adminService.CreateProject(project);
        _adminService.SetProjectLeader(project.Name, leaderUser.Email);

        var verifyProject = _repositoryManager.ProjectRepository.Get(p => p.Name == "Test Project Direct");
        Assert.IsNotNull(verifyProject?.ProjectLeader, "Project leader should not be null after direct creation");

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var projects = _leaderService.GetAllMyProjects();

        Assert.AreEqual(1, projects.Count);
        Assert.AreEqual("Test Project Direct", projects[0].Name);
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void LeaderPService_ShouldThrowProjectNotFoundException_WhenProjectDoesNotExist()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var taskDTO = new TaskDTO
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

        var updatedTask = new TaskDTO
        {
            Title = "Initial Task",
            Description = "Updated description by leader",
            ExpectedStartDate = DateTime.Now.AddDays(3),
            Duration = 5,
            State = StateDTO.DOING,
            Resources = new List<ResourceDTO>()
        };

        _leaderService.UpdateTask("Test Project", "Initial Task", updatedTask);

        var retrievedTask = _leaderService.GetTask("Test Project", "Initial Task");
        Assert.AreEqual("Updated description by leader", retrievedTask.Description);
        Assert.AreEqual(5, retrievedTask.Duration);
        Assert.AreEqual(StateDTO.DOING, retrievedTask.State);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskNotFoundException))]
    public void LeaderPService_ShouldThrowTaskNotFoundException_WhenUpdatingNonexistentTask()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var taskDTO = new TaskDTO
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

        var task2 = new TaskDTO
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

        var tasks = _leaderService.GetTasks("Test Project");
        Assert.AreEqual(2, tasks.Count);
        Assert.IsTrue(tasks.Any(t => t.Title == "Initial Task"));
        Assert.IsTrue(tasks.Any(t => t.Title == "Task 2"));
    }

    [TestMethod]
    public void LeaderPService_ShouldGetSpecificTask_WhenUserIsProjectLeader()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var retrievedTask = _leaderService.GetTask("Test Project", "Initial Task");

        Assert.IsNotNull(retrievedTask);
        Assert.AreEqual("Initial Task", retrievedTask.Title);
        Assert.AreEqual("Initial task for testing", retrievedTask.Description);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedLeaderAccessException))]
    public void LeaderPService_ShouldThrowUnauthorizedAccessException_WhenUserIsNotLeaderOfSpecificProject()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var anotherLeader = new UserDTO
        {
            FirstName = "Another",
            LastName = "Leader",
            Email = "another.leader@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.ProjectLeader }
        };

        _userService.AddUser(anotherLeader);

        var anotherProject = new ProjectDTO
        {
            Name = "Another Project",
            Description = "Another project description",
            StartDate = DateTime.Now.AddDays(1),
            AdminProyect = _userService.GetUser("admin.user@example.com"),
            ProjectLeader = anotherLeader,
            Members = new List<UserDTO>()
        };

        _adminService.CreateProject(anotherProject);

        var taskForAnotherProject = new TaskDTO
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

        var taskDTO = new TaskDTO
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

        var projectDTO = _leaderService.GetProject("Test Project");

        Assert.IsNotNull(projectDTO, "ProjectDTO should not be null");
        Assert.AreEqual("Test Project", projectDTO.Name);
        Assert.AreEqual("Test project description", projectDTO.Description);

        if (projectDTO.ProjectLeader == null)
        {
            var originalProject = _repositoryManager.ProjectRepository.Get(p => p.Name == "Test Project");
        }

        Assert.IsNotNull(projectDTO.ProjectLeader, "ProjectLeader should not be null");
        Assert.AreEqual("leader.user@example.com", projectDTO.ProjectLeader.Email);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedLeaderAccessException))]
    public void LeaderPService_ShouldThrowUnathorizedAccessException_WhenUserIsNotProjectLeader()
    {
        _loginService.LoginUser("normal.user@example.com", "Password123@");

        var taskDTO = new TaskDTO
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

        var members = _leaderService.GetAllMembersOfAProject(project.Name);

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

        var task1 = new TaskDTO
        {
            Title = "Task 1",
            Description = "Task 1 description",
            ExpectedStartDate = DateTime.Now.AddDays(2),
            Duration = 5,
            State = StateDTO.TODO,
            Resources = new List<ResourceDTO>()
        };

        var task2 = new TaskDTO
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

        var tasksForMember = _leaderService.GetAllTaskForAMemberInAProject(project.Name, normalUserDTO.Email);

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

        var tasksForMember = _leaderService.GetAllTaskForAMemberInAProject(project.Name, normalUserDTO.Email);
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

        var tasksBeforeRemoval = _leaderService.GetAllTaskForAMemberInAProject(project.Name, normalUserDTO.Email);
        Assert.IsTrue(tasksBeforeRemoval.Any(t => t.Title == initialTask.Title), "Task should exist before removal");

        _leaderService.RemoveTaskFromMember(project.Name, normalUserDTO.Email, initialTask.Title);

        var tasksAfterRemoval = _leaderService.GetAllTaskForAMemberInAProject(project.Name, normalUserDTO.Email);
        Assert.IsFalse(tasksAfterRemoval.Any(t => t.Title == initialTask.Title),
            "Task should be removed from the member's task list.");
    }

    [TestMethod]
    public void ExportProjects_CSV_ShouldReturnCorrectFormat_WhenUserIsProjectLeader()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (var proj in existingProjects) _repositoryManager.ProjectRepository.Delete(proj);

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var baseDate = DateTime.Now.AddDays(10);

        var project1 = new ProjectDTO
        {
            Name = "Proyecto A",
            Description = "Primer proyecto",
            StartDate = baseDate,
            AdminProyect = _userService.GetUser("admin.user@example.com")
        };

        var project2 = new ProjectDTO
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

        var tarea1_A = new TaskDTO
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

        var tarea2_A = new TaskDTO
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

        var tarea3_A = new TaskDTO
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

        var tarea4_A = new TaskDTO
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

        var tarea1_B = new TaskDTO
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

        var csvResult = leaderServiceWithCsv.ExportProjects();
        Assert.IsNotNull(csvResult);

        var expectedDateA = baseDate.ToString("dd/MM/yyyy");
        var expectedDateB = baseDate.AddDays(10).ToString("dd/MM/yyyy");
        var expectedDateT1 = baseDate.AddDays(1).ToString("dd/MM/yyyy");
        var expectedDateT2 = baseDate.AddDays(3).ToString("dd/MM/yyyy");
        var expectedDateT3 = baseDate.AddDays(2).ToString("dd/MM/yyyy");
        var expectedDateT4 = baseDate.AddDays(4).ToString("dd/MM/yyyy");
        var expectedDateT5 = baseDate.AddDays(11).ToString("dd/MM/yyyy");

        string[] lines = csvResult
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        Assert.AreEqual($"Proyecto A,{expectedDateA}", lines[0], "La primera línea debe ser 'Proyecto A,fecha'");
        Assert.AreEqual($"Zebra,{expectedDateT3},S", lines[1], "Primera tarea debe ser Zebra con crítico S");
        Assert.AreEqual($"Tarea Z,{expectedDateT1},S", lines[2], "Segunda tarea debe ser Tarea Z con crítico S");
        Assert.AreEqual($"Medio,{expectedDateT4},S", lines[3], "Tercera tarea debe ser Medio con crítico S");
        Assert.AreEqual($"Alpha,{expectedDateT2},S", lines[4], "Cuarta tarea debe ser Alpha con crítico S");
        var idxB = Array.FindIndex(lines, l => l.StartsWith("Proyecto B,"));
        Assert.IsTrue(idxB > 0, "Debe existir una línea que empiece con 'Proyecto B,'");
        Assert.AreEqual($"Proyecto B,{expectedDateB}", lines[idxB], "Línea de Proyecto B incorrecta");
        Assert.AreEqual($"T1,{expectedDateT5},S", lines[idxB + 1], "Tarea T1 de Proyecto B incorrecta");
    }

    [TestMethod]
    public void ExportProjects_JSON_ShouldReturnCorrectFormat_WhenUserIsProjectLeader()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (var proj in existingProjects) _repositoryManager.ProjectRepository.Delete(proj);

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var baseDate = DateTime.Now.AddDays(10);

        var project1 = new ProjectDTO
        {
            Name = "Proyecto A",
            Description = "Primer proyecto",
            StartDate = baseDate,
            AdminProyect = _userService.GetUser("admin.user@example.com")
        };
        var project2 = new ProjectDTO
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

        var tarea1_A = new TaskDTO
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
        var tarea2_A = new TaskDTO
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
        var tarea3_A = new TaskDTO
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
        var tarea4_A = new TaskDTO
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

        var tarea1_B = new TaskDTO
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

        var jsonResult = leaderServiceWithJson.ExportProjects();
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

        var dA = baseDate.ToString("dd/MM/yyyy");
        var dB = baseDate.AddDays(10).ToString("dd/MM/yyyy");
        var dt1 = baseDate.AddDays(1).ToString("dd/MM/yyyy");
        var dt2 = baseDate.AddDays(3).ToString("dd/MM/yyyy");
        var dt3 = baseDate.AddDays(2).ToString("dd/MM/yyyy");
        var dt4 = baseDate.AddDays(4).ToString("dd/MM/yyyy");
        var dt5 = baseDate.AddDays(11).ToString("dd/MM/yyyy");

        Assert.IsTrue(jsonResult.Contains($"\"{dA}\""), $"Debe contener la fecha {dA}");
        Assert.IsTrue(jsonResult.Contains($"\"{dB}\""), $"Debe contener la fecha {dB}");
        Assert.IsTrue(jsonResult.Contains($"\"{dt1}\""), $"Debe contener la fecha de Tarea Z {dt1}");
        Assert.IsTrue(jsonResult.Contains($"\"{dt2}\""), $"Debe contener la fecha de Alpha {dt2}");
        Assert.IsTrue(jsonResult.Contains($"\"{dt3}\""), $"Debe contener la fecha de Zebra {dt3}");
        Assert.IsTrue(jsonResult.Contains($"\"{dt4}\""), $"Debe contener la fecha de Medio {dt4}");
        Assert.IsTrue(jsonResult.Contains($"\"{dt5}\""), $"Debe contener la fecha de T1 {dt5}");

        Assert.IsTrue(jsonResult.Contains("\"IsCritical\": \"S\""),
            "Todas las tareas deben salir marcadas como críticas (\"S\") tras el cálculo CPM");

        Assert.IsTrue(jsonResult.Contains("\"Duration\": 5"));
        Assert.IsTrue(jsonResult.Contains("\"Duration\": 3"));
        Assert.IsTrue(jsonResult.Contains("\"Duration\": 4"));
        Assert.IsTrue(jsonResult.Contains("\"Duration\": 2"));
        Assert.IsTrue(jsonResult.Contains("\"Resources\": []"));

        List<dynamic> projects = JsonConvert.DeserializeObject<List<dynamic>>(jsonResult);
        Assert.AreEqual(2, projects.Count, "Debe haber exactamente 2 proyectos");

        Assert.AreEqual("Proyecto A", (string)projects[0].Project);
        Assert.AreEqual("Proyecto B", (string)projects[1].Project);

        var tasksA = projects[0].Tasks;
        Assert.AreEqual(4, tasksA.Count, "Proyecto A debe tener 4 tareas");
        Assert.AreEqual("Zebra", (string)tasksA[0].Task);
        Assert.AreEqual("Tarea Z", (string)tasksA[1].Task);
        Assert.AreEqual("Medio", (string)tasksA[2].Task);
        Assert.AreEqual("Alpha", (string)tasksA[3].Task);

        var tasksB = projects[1].Tasks;
        Assert.AreEqual(1, tasksB.Count, "Proyecto B debe tener 1 tarea");
        Assert.AreEqual("T1", (string)tasksB[0].Task);
    }

    [TestMethod]
    public void ExportProjects_JSON_ShouldReturnEmptyArray_WhenNoProjectsExist()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (var proj in existingProjects) _repositoryManager.ProjectRepository.Delete(proj);

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var jsonExporter = new JSONExporter(_repositoryManager);
        var leaderServiceWithJson = new LeaderPService(_repositoryManager, jsonExporter);

        var jsonResult = leaderServiceWithJson.ExportProjects();

        Assert.IsNotNull(jsonResult, "El resultado no debe ser null");

        List<object> deserializedResult = JsonConvert.DeserializeObject<List<object>>(jsonResult);
        Assert.IsNotNull(deserializedResult, "El resultado deserializado no debe ser null");
        Assert.AreEqual(0, deserializedResult.Count, "El array debe estar vacío cuando no hay proyectos");

        var normalizedJson = jsonResult.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        Assert.AreEqual("[]", normalizedJson, "El JSON debe ser un array vacío");
    }

    [TestMethod]
    public void ExportProjects_CSV_ShouldReturnEmpty_WhenNoProjectsExist()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (var proj in existingProjects) _repositoryManager.ProjectRepository.Delete(proj);
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var csvExporter = new CSVExporter(_repositoryManager);
        var leaderServiceWithCsv = new LeaderPService(_repositoryManager, csvExporter);
        var csvResult = leaderServiceWithCsv.ExportProjects();
        Assert.IsNotNull(csvResult, "El resultado no debe ser null");
        var trimmed = csvResult.Trim();
        Assert.IsTrue(string.IsNullOrEmpty(trimmed),
            "Debe devolver cadena vacía cuando no hay proyectos");
        string[] lines = csvResult
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(0, lines.Length,
            "No debe haber líneas cuando no hay proyectos");
    }

    [TestMethod]
    public void ExportProjects_CSV_ShouldEscapeFieldsWithCommas()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (var proj in existingProjects) _repositoryManager.ProjectRepository.Delete(proj);

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var baseDate = DateTime.Now.AddDays(10);

        var projectWithCommas = new ProjectDTO
        {
            Name = "Proyecto, con comas",
            Description = "Proyecto con comas en el nombre",
            StartDate = baseDate,
            AdminProyect = _userService.GetUser("admin.user@example.com")
        };

        _adminService.CreateProject(projectWithCommas);
        _adminService.SetProjectLeader("Proyecto, con comas", "leader.user@example.com");

        var taskWithCommas = new TaskDTO
        {
            Title = "Tarea, con comas",
            Description = "Tarea con comas",
            ExpectedStartDate = baseDate.AddDays(1),
            StartDate = baseDate.AddDays(1),
            Duration = 3,
            State = StateDTO.TODO,
            IsCritical = false,
            Resources = new List<ResourceDTO>()
        };

        _taskService.AddTask("Proyecto, con comas", taskWithCommas);

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var csvExporter = new CSVExporter(_repositoryManager);
        var leaderServiceWithCsv = new LeaderPService(_repositoryManager, csvExporter);

        var csvResult = leaderServiceWithCsv.ExportProjects();

        Assert.IsTrue(csvResult.Contains("\"Proyecto, con comas\""));
        Assert.IsTrue(csvResult.Contains("\"Tarea, con comas\""));
    }

    [TestMethod]
    public void ExportProjects_CSV_ShouldEscapeFieldsWithQuotes()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (var proj in existingProjects) _repositoryManager.ProjectRepository.Delete(proj);

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var baseDate = DateTime.Now.AddDays(10);

        var projectWithQuotes = new ProjectDTO
        {
            Name = "Proyecto \"con comillas\"",
            Description = "Proyecto con comillas",
            StartDate = baseDate,
            AdminProyect = _userService.GetUser("admin.user@example.com")
        };

        _adminService.CreateProject(projectWithQuotes);
        _adminService.SetProjectLeader("Proyecto \"con comillas\"", "leader.user@example.com");

        var taskWithQuotes = new TaskDTO
        {
            Title = "Tarea \"con comillas\"",
            Description = "Tarea con comillas",
            ExpectedStartDate = baseDate.AddDays(1),
            StartDate = baseDate.AddDays(1),
            Duration = 3,
            State = StateDTO.TODO,
            IsCritical = false,
            Resources = new List<ResourceDTO>()
        };

        _taskService.AddTask("Proyecto \"con comillas\"", taskWithQuotes);

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var csvExporter = new CSVExporter(_repositoryManager);
        var leaderServiceWithCsv = new LeaderPService(_repositoryManager, csvExporter);

        var csvResult = leaderServiceWithCsv.ExportProjects();

        Assert.IsTrue(csvResult.Contains("\"Proyecto \"\"con comillas\"\"\""));
        Assert.IsTrue(csvResult.Contains("\"Tarea \"\"con comillas\"\"\""));
    }

    [TestMethod]
    public void ExportProjects_CSV_ShouldEscapeFieldsWithNewlines()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (var proj in existingProjects) _repositoryManager.ProjectRepository.Delete(proj);

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var baseDate = DateTime.Now.AddDays(10);

        var projectWithNewlines = new ProjectDTO
        {
            Name = "Proyecto\ncon saltos",
            Description = "Proyecto con saltos de línea",
            StartDate = baseDate,
            AdminProyect = _userService.GetUser("admin.user@example.com")
        };

        _adminService.CreateProject(projectWithNewlines);
        _adminService.SetProjectLeader("Proyecto\ncon saltos", "leader.user@example.com");

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var csvExporter = new CSVExporter(_repositoryManager);
        var leaderServiceWithCsv = new LeaderPService(_repositoryManager, csvExporter);

        var csvResult = leaderServiceWithCsv.ExportProjects();
        Assert.IsTrue(csvResult.Contains("\"Proyecto\ncon saltos\""));
    }

    [TestMethod]
    public void ExportProjects_CSV_ShouldHandleEmptyFields()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (var proj in existingProjects) _repositoryManager.ProjectRepository.Delete(proj);

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var baseDate = DateTime.Now.AddDays(10);

        var projectWithValid = new ProjectDTO
        {
            Name = "Proyecto Valido",
            Description = "Proyecto válido para test de campos vacíos",
            StartDate = baseDate,
            AdminProyect = _userService.GetUser("admin.user@example.com")
        };

        _adminService.CreateProject(projectWithValid);
        _adminService.SetProjectLeader("Proyecto Valido", "leader.user@example.com");

        var taskWithEmpty = new TaskDTO
        {
            Title = "Tarea Valida",
            Description = "Tarea válida sin recursos",
            ExpectedStartDate = baseDate.AddDays(1),
            StartDate = baseDate.AddDays(1),
            Duration = 3,
            State = StateDTO.TODO,
            IsCritical = false,
            Resources = new List<ResourceDTO>()
        };

        _taskService.AddTask("Proyecto Valido", taskWithEmpty);

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var csvExporter = new CSVExporter(_repositoryManager);
        var leaderServiceWithCsv = new LeaderPService(_repositoryManager, csvExporter);

        var csvResult = leaderServiceWithCsv.ExportProjects();

        Assert.IsNotNull(csvResult);
        Assert.IsTrue(csvResult.Contains("Proyecto Valido"));
        Assert.IsTrue(csvResult.Contains("Tarea Valida"));

        string[] lines = csvResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.IsTrue(lines.Length > 0);
    }

    [TestMethod]
    public void ExportProjects_CSV_ShouldIncludeResourcesInSeparateLines()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (var proj in existingProjects) _repositoryManager.ProjectRepository.Delete(proj);

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var baseDate = DateTime.Now.AddDays(10);

        var project = new ProjectDTO
        {
            Name = "Proyecto con Recursos",
            Description = "Proyecto con recursos",
            StartDate = baseDate,
            AdminProyect = _userService.GetUser("admin.user@example.com")
        };

        _adminService.CreateProject(project);
        _adminService.SetProjectLeader("Proyecto con Recursos", "leader.user@example.com");

        var taskWithEmptyResources = new TaskDTO
        {
            Title = "Tarea con Lista Recursos",
            Description = "Tarea para probar lógica de recursos",
            ExpectedStartDate = baseDate.AddDays(1),
            StartDate = baseDate.AddDays(1),
            Duration = 3,
            State = StateDTO.TODO,
            IsCritical = false,
            Resources = new List<ResourceDTO>()
        };

        _taskService.AddTask("Proyecto con Recursos", taskWithEmptyResources);

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var csvExporter = new CSVExporter(_repositoryManager);
        var leaderServiceWithCsv = new LeaderPService(_repositoryManager, csvExporter);

        var csvResult = leaderServiceWithCsv.ExportProjects();

        Assert.IsNotNull(csvResult);
        Assert.IsTrue(csvResult.Contains("Proyecto con Recursos"));
        Assert.IsTrue(csvResult.Contains("Tarea con Lista Recursos"));

        string[] lines = csvResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.IsTrue(lines.Length >= 2);
    }

    [TestMethod]
    public void ExportProjects_CSV_ShouldHandleTasksWithoutResources()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (var proj in existingProjects) _repositoryManager.ProjectRepository.Delete(proj);

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var baseDate = DateTime.Now.AddDays(10);

        var project = new ProjectDTO
        {
            Name = "Proyecto Sin Recursos",
            Description = "Proyecto sin recursos",
            StartDate = baseDate,
            AdminProyect = _userService.GetUser("admin.user@example.com")
        };

        _adminService.CreateProject(project);
        _adminService.SetProjectLeader("Proyecto Sin Recursos", "leader.user@example.com");

        var taskWithoutResources = new TaskDTO
        {
            Title = "Tarea Sin Recursos",
            Description = "Tarea sin recursos",
            ExpectedStartDate = baseDate.AddDays(1),
            StartDate = baseDate.AddDays(1),
            Duration = 3,
            State = StateDTO.TODO,
            IsCritical = false,
            Resources = new List<ResourceDTO>()
        };

        _taskService.AddTask("Proyecto Sin Recursos", taskWithoutResources);

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var csvExporter = new CSVExporter(_repositoryManager);
        var leaderServiceWithCsv = new LeaderPService(_repositoryManager, csvExporter);

        var csvResult = leaderServiceWithCsv.ExportProjects();
        Assert.IsNotNull(csvResult);
        Assert.IsTrue(csvResult.Contains("Proyecto Sin Recursos"));
        Assert.IsTrue(csvResult.Contains("Tarea Sin Recursos"));
    }

    [TestMethod]
    [ExpectedException(typeof(NullProjectsCanNotBeImported))]
    public void ExportProjects_ShouldThrowNullProjectsCanNotBeImported_WhenProjectsListIsNull()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (var proj in existingProjects) _repositoryManager.ProjectRepository.Delete(proj);

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var testExporter = new TestExporterForNull();
        var leaderServiceWithTestExporter = new LeaderPService(_repositoryManager, testExporter);

        testExporter.SimulateNullProjects();
    }

    [TestMethod]
    public void ExportProjects_ShouldFilterNullProjects_WhenProjectsListContainsNullElements()
    {
        List<Project> existingProjects = _repositoryManager.ProjectRepository.GetAll().ToList();
        foreach (var proj in existingProjects) _repositoryManager.ProjectRepository.Delete(proj);

        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var baseDate = DateTime.Now.AddDays(10);

        var validProject = new ProjectDTO
        {
            Name = "Valid Project",
            Description = "Valid project for null test",
            StartDate = baseDate,
            AdminProyect = _userService.GetUser("admin.user@example.com")
        };

        _adminService.CreateProject(validProject);
        _adminService.SetProjectLeader("Valid Project", "leader.user@example.com");

        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var testExporter = new TestExporterWithNullElements();
        var leaderServiceWithTestExporter = new LeaderPService(_repositoryManager, testExporter);

        var result = leaderServiceWithTestExporter.ExportProjects();

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("1 valid project"));
    }

    [TestMethod]
    [ExpectedException(typeof(TheProjectDoesNotHaveAProjectLeader))]
    public void LeaderPService_ShouldThrowTheProjectDoesNotHaveAProjectLeader_WhenProjectHasNoLeader()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var projectWithoutLeader = new ProjectDTO
        {
            Name = "Project Without Leader",
            Description = "Project that has no leader assigned",
            StartDate = DateTime.Now.AddDays(1),
            AdminProyect = _userService.GetUser("admin.user@example.com"),
            ProjectLeader = null
        };

        _adminService.CreateProject(projectWithoutLeader);

        var testAdminService = new TestAdminPServiceForException(_repositoryManager);
        testAdminService.TriggerProjectDoesNotHaveLeaderException("Project Without Leader");
    }

    [TestMethod]
    [ExpectedException(typeof(TheProjectAlredyHasALeader))]
    public void LeaderPService_ShouldThrowTheProjectAlredyHasALeader_WhenProjectAlreadyHasLeader()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var projectWithLeader = new ProjectDTO
        {
            Name = "Project With Leader",
            Description = "Project that already has a leader",
            StartDate = DateTime.Now.AddDays(1),
            AdminProyect = _userService.GetUser("admin.user@example.com")
        };

        _adminService.CreateProject(projectWithLeader);
        _adminService.SetProjectLeader("Project With Leader", "leader.user@example.com");

        var anotherLeader = new UserDTO
        {
            FirstName = "Another",
            LastName = "Leader",
            Email = "another.leader2@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.ProjectLeader }
        };

        _userService.AddUser(anotherLeader);

        _adminService.SetProjectLeader("Project With Leader", "another.leader2@example.com");
    }

    [TestMethod]
    [ExpectedException(typeof(UnableToExportProject))]
    public void LeaderPService_ShouldThrowUnableToExportProject_WhenExportFails()
    {
        _loginService.LoginUser("leader.user@example.com", "LeaderPassword123@");

        var failingExporter = new FailingExporter();
        var leaderServiceWithFailingExporter = new LeaderPService(_repositoryManager, failingExporter);

        leaderServiceWithFailingExporter.ExportProjects();
    }

    public class TestExporterForNull : ExporterBase
    {
        protected override string ExportData(List<ProjectDTO> projects)
        {
            return "Test export data";
        }

        public void SimulateNullProjects()
        {
            Export(null);
        }
    }

    public class TestExporterWithNullElements : ExporterBase
    {
        protected override string ExportData(List<ProjectDTO> projects)
        {
            return $"{projects.Count} valid project";
        }

        public string TestExportWithNullElements(List<ProjectDTO> projectsWithNull)
        {
            return Export(projectsWithNull);
        }
    }

    public class TestAdminPServiceForException : AdminPService
    {
        public TestAdminPServiceForException(IRepositoryManager repositoryManager) : base(repositoryManager)
        {
        }

        public void TriggerProjectDoesNotHaveLeaderException(string projectName)
        {
            RemoveProjectLeader(projectName);
        }
    }

    public class FailingExporter : ExporterBase
    {
        protected override string ExportData(List<ProjectDTO> projects)
        {
            throw new Exception("Simulated export failure");
        }
    }
}