using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Domain;

namespace Service.Test;

[TestClass]
public class AdminPServiceTests
{
    private AdminPService _adminPservice;
    private AppDbContext _context;
    private Login _login;
    private IRepositoryManager _repositoryManager;
    private TaskService _taskService;
    private UserService _userservice;
    private UserDTO Admin;
    private UserDTO Leader;
    private List<UserDTO> members;
    private UserDTO UserDTO;

    [TestInitialize]
    public void Setup()
    {
        var contextFactory = new InMemoryAppContextFactory();
        _context = contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repositoryManager = new RepositoryManager(_context);

        var cpmService = new CpmService();
        _taskService = new TaskService(_repositoryManager, cpmService);
        _adminPservice = new AdminPService(_repositoryManager);

        _userservice = new UserService(_repositoryManager);
        _login = new Login(_repositoryManager);

        Admin = new UserDTO
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin.user@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };

        UserDTO = new UserDTO
        {
            FirstName = "User",
            LastName = "Member",
            Email = "member.user@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        Leader = new UserDTO
        {
            FirstName = "Normal",
            LastName = "User",
            Email = "normal.user@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<RolDTO> { RolDTO.ProjectLeader }
        };

        members = new List<UserDTO> { UserDTO };

        _userservice.AddUser(Admin);
        _userservice.AddUser(UserDTO);
        _userservice.AddUser(Leader);
        _login.LoginUser(Admin.Email, Admin.Password);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Database.EnsureDeleted();
    }

    [TestMethod]
    public void CreateProject_ShouldAddProjectToDatabase_WhenValid()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Today,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        Assert.IsNotNull(project);
        Assert.AreEqual("New Project", project.Name);
    }

    [TestMethod]
    public void AssignMembersToProject_ShouldAddMembers_WhenValid()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        var newUserDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123!",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        var userService = new UserService(_repositoryManager);
        userService.AddUser(newUserDTO);

        var projectBeforeAdd = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        var initialMemberCount = projectBeforeAdd.Members?.Count ?? 0;

        _adminPservice.AssignMembersToProject(projectDTO.Name, new List<UserDTO> { newUserDTO });

        var projectAfterAdd = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);

        Assert.IsNotNull(projectAfterAdd.Members, "Members list should not be null");
        Assert.AreEqual(initialMemberCount + 1, projectAfterAdd.Members.Count, "Should have one more member");

        var addedUser = projectAfterAdd.Members.FirstOrDefault(m => m.Email == "john.doe@example.com");
        Assert.IsNotNull(addedUser, "John should be found in the members list");
        Assert.AreEqual("John", addedUser.FirstName, "First name should match");
        Assert.AreEqual("Doe", addedUser.LastName, "Last name should match");
    }

    [TestMethod]
    [ExpectedException(typeof(UserIsAlreadyAMemberException))]
    public void AssignMembersToProject_ShouldThrowException_WhenAddingAMemberThatAlreadyExists()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        var userDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123!",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        var userService = new UserService(_repositoryManager);
        userService.AddUser(userDTO);

        _adminPservice.AssignMembersToProject(projectDTO.Name, new List<UserDTO> { userDTO });

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        Assert.IsTrue(project.Members.Any(m => m.Email == "john.doe@example.com"),
            "User should be added to project after first call");

        _adminPservice.AssignMembersToProject(projectDTO.Name, new List<UserDTO> { userDTO });

        Assert.Fail("Expected UserIsAlreadyAMemberException was not thrown");
    }

    [TestMethod]
    public void RemoveMembersFromProject_ShouldRemoveMember_WhenMemberExists()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = new List<UserDTO>()
        };

        _adminPservice.CreateProject(projectDTO);

        var johnDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "johndoe@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123!",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        var memberDTO = new UserDTO
        {
            FirstName = "Member",
            LastName = "User",
            Email = "memberuser@example.com",
            Birthday = DateTime.Parse("1985-05-15"),
            Password = "Password123!",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        var userService = new UserService(_repositoryManager);
        userService.AddUser(johnDTO);
        userService.AddUser(memberDTO);

        _adminPservice.AssignMembersToProject(projectDTO.Name, new List<UserDTO> { johnDTO, memberDTO });

        var projectAfterAdd = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);

        Assert.IsTrue(projectAfterAdd.Members.Any(m => m.Email == "johndoe@example.com"),
            "John should be in the project");
        Assert.IsTrue(projectAfterAdd.Members.Any(m => m.Email == "memberuser@example.com"),
            "Member should be in the project");
        Assert.AreEqual(2, projectAfterAdd.Members.Count, "Should have exactly 2 members");

        var johnInProject = projectAfterAdd.Members.First(m => m.Email == "johndoe@example.com");
        Assert.AreEqual("John", johnInProject.FirstName, "John's first name should be correct");

        _adminPservice.RemoveMemberFromProject(projectDTO.Name, "johndoe@example.com");
        _adminPservice.RemoveMemberFromProject(projectDTO.Name, "memberuser@example.com");

        var projectAfterRemove = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);

        Assert.IsFalse(projectAfterRemove.Members.Any(m => m.Email == "johndoe@example.com"),
            "John should be removed from the project");
        Assert.IsFalse(projectAfterRemove.Members.Any(m => m.Email == "memberuser@example.com"),
            "Member should be removed from the project");
        Assert.AreEqual(0, projectAfterRemove.Members.Count,
            "Project should have no members after removing all");
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void RemoveMembersFromProject_ShouldThrowException_WhenRemovingMemberFromAProjectThatNotExist()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        _adminPservice.RemoveMemberFromProject("Proyecto 1", "member.user@example.com");
    }

    [TestMethod]
    [ExpectedException(typeof(UserIsNotAMemberException))]
    public void RemoveMembersFromProject_ShouldThrowException_WhenRemovingMemberThatNotExists()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        _adminPservice.RemoveMemberFromProject(projectDTO.Name, "john.user@example.com");
    }

    [TestMethod]
    public void RemoveProject_ShouldRemoveProject_WhenValid()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        Assert.IsNotNull(project);

        _adminPservice.RemoveProject("New Project");

        project = _repositoryManager.ProjectRepository.Get(p => p.Name == "New Project");
        Assert.IsNull(project);
    }

    [TestMethod]
    public void UpdateProject_ShouldUpdateProject_WhenValid()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Old Project",
            Description = "Old Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectDTO.Name);
        Assert.IsNotNull(project);

        var updatedDTO = new ProjectDTO
        {
            Name = "Updated Project",
            Description = "Updated Description",
            StartDate = DateTime.Now.AddDays(1),
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.UpdateProject("Old Project", updatedDTO);
    }

    [TestMethod]
    public void GetAllProjects_ShouldReturnAllProjects()
    {
        var projectDTO1 = new ProjectDTO
        {
            Name = "Project 1",
            Description = "Description 1",
            StartDate = DateTime.Now,
            Members = members
        };
        var projectDTO2 = new ProjectDTO
        {
            Name = "Project 2",
            Description = "Description 2",
            StartDate = DateTime.Now.AddDays(1),
            Members = members
        };

        _adminPservice.CreateProject(projectDTO1);
        _adminPservice.CreateProject(projectDTO2);

        var projects = _adminPservice.GetAllProjects();

        Assert.AreEqual(2, projects.Count);
        Assert.AreEqual("Project 1", projects[0].Name);
        Assert.AreEqual("Project 2", projects[1].Name);
    }

    [TestMethod]
    public void GetProjectByName_ShouldReturnProject_WhenProjectExists()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        var project = _adminPservice.GetProjectByName("Test Project");

        Assert.IsNotNull(project);
        Assert.AreEqual("Test Project", project.Name);
        Assert.AreEqual("Test Description", project.Description);
    }

    [TestMethod]
    public void GetAllProjectsForUsers_ShouldReturnListOfProjects_WhenUserIsAdminOrMember()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        var project = _adminPservice.GetAllProjectsForUser(UserDTO.Email);

        Assert.IsNotNull(project);
        Assert.AreEqual("Test Project", project[0].Name);
        Assert.AreEqual("Test Description", project[0].Description);
    }

    [TestMethod]
    public void GetMembers_ShouldReturnListOfMembers_WhenProjectExist()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        var projectMembers = _adminPservice.GetMembers("Test Project");
        Assert.IsNotNull(projectMembers);
        Assert.AreEqual(1, projectMembers.Count);
        Assert.AreEqual("User", projectMembers[0].FirstName);
    }

    [TestMethod]
    public void AddTaskToMember_WhenUserIsMember_ShouldAddTaskToMember()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };

        _taskService.AddTask("Test Project", task);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task.Title);

        Assert.IsTrue(_userservice.GetUser("member.user@example.com").Tasks.Any(t => t.Id == 1));
    }

    [TestMethod]
    [ExpectedException(typeof(UserIsNotAMemberException))]
    public void AddTaskToMember_WhenUserIsNotMember_ShouldThrowUserIsNotAMemberException()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        _taskService.AddTask("Test Project", task);

        var addedTasks = _taskService.GetTasks("Test Project");
        var addedTask = addedTasks.FirstOrDefault(t => t.Title == "Task1");

        _adminPservice.AddTaskToMember("Test Project", "member1.user@example.com", addedTask.Title);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskIsNotFromTheProjectException))]
    public void AddTaskToMember_WhenTaskIsNotFromTheProject_ShouldThrowException()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };

        _taskService.AddTask("Test Project", task);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", "task2");
    }

    [TestMethod]
    public void RemoveTaskFromMember_WhenUserIsMember_ShouldRemoveTaskFromMember()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };

        _taskService.AddTask("Test Project", task);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task.Title);

        Assert.IsTrue(_userservice.GetUser("member.user@example.com").Tasks.Any(t => t.Id == 1));

        _adminPservice.RemoveTaskFromMember("Test Project", "member.user@example.com", task.Title);

        Assert.IsTrue(_userservice.GetUser("member.user@example.com").Tasks.Count == 0);
    }

    [TestMethod]
    [ExpectedException(typeof(UserIsNotAMemberException))]
    public void RemoveTaskFromMember_WhenUserIsNotMember_ShouldThrowUserIsNotAMemberException()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };

        _taskService.AddTask("Test Project", task);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task.Title);
        _adminPservice.RemoveTaskFromMember("Test Project", "member1.user@example.com", task.Title);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskIsNotFromTheProjectException))]
    public void RemoveTaskFromMember_WhenTaskIsNotFromTheProject_ShouldThrowException()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };


        _taskService.AddTask("Test Project", task);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task.Title);
        _adminPservice.RemoveTaskFromMember("Test Project", "member.user@example.com", "task2");
    }

    [TestMethod]
    public void GetTasksForAMember_WhenGettingTasksForAMember_ShouldReturnListOfTasks()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        var task2 = new TaskDTO
        {
            Title = "Task2",
            Description = "Description2",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        _taskService.AddTask("Test Project", task);
        _taskService.AddTask("Test Project", task2);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task.Title);
        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task2.Title);

        var tasks = _adminPservice.GetAllTaskForAMember("member.user@example.com");

        Assert.AreEqual(2, tasks.Count);
    }

    [TestMethod]
    public void GetTasksForAMemberInAProject_WhenGettingTasksForAMember_ShouldReturnListOfTasks()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);
        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        var task2 = new TaskDTO
        {
            Title = "Task2",
            Description = "Description2",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };

        _taskService.AddTask("Test Project", task);
        _taskService.AddTask("Test Project", task2);

        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task.Title);
        _adminPservice.AddTaskToMember("Test Project", "member.user@example.com", task2.Title);

        var tasks = _adminPservice.GetAllTaskForAMemberInAProject("Test Project", "member.user@example.com");

        Assert.AreEqual(2, tasks.Count);
    }

    [TestMethod]
    public void SetProjectLeader_WhenTheUserIsNotAlredyProjectLeader_ShouldBeOkey()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);

        _adminPservice.SetProjectLeader(projectDTO.Name, Leader.Email);
    }

    [TestMethod]
    public void SetProjectLeader_ShouldAssignLeader_WhenValid()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        _adminPservice.SetProjectLeader("New Project", Leader.Email);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "New Project");
        Assert.AreEqual(Leader.Email, project.ProjectLeader.Email);
    }

    [TestMethod]
    public void GetAllProjectLeaderUsers_ShouldReturnOnlyProjectLeaders()
    {
        var projectLeaders = _adminPservice.GetAllProjectLeaderUsers();

        Assert.AreEqual(1, projectLeaders.Count, "There should be exactly one project leader");
        Assert.AreEqual(Leader.FirstName, projectLeaders[0].FirstName,
            "The first leader should be the one with the correct name");
        Assert.AreEqual(Leader.Email, projectLeaders[0].Email, "The leader should have the correct email");
    }

    [TestMethod]
    public void GetTasks_ShouldReturnAllTasks_WhenProjectHasTasks()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPservice.CreateProject(projectDTO);

        var task1 = new TaskDTO
        {
            Title = "Task 1",
            Description = "Task 1 Description",
            Duration = 2,
            ExpectedStartDate = DateTime.Today
        };
        var task2 = new TaskDTO
        {
            Title = "Task 2",
            Description = "Task 2 Description",
            Duration = 3,
            ExpectedStartDate = DateTime.Today.AddDays(1)
        };

        _taskService.AddTask("Test Project", task1);
        _taskService.AddTask("Test Project", task2);

        var tasks = _adminPservice.GetTasks(projectDTO);

        Assert.AreEqual(2, tasks.Count);
        Assert.AreEqual("Task 1", tasks[0].Title);
        Assert.AreEqual("Task 2", tasks[1].Title);
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void GetTasks_ShouldThrowException_WhenProjectDoesNotExist()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Nonexistent Project",
            Description = "Nonexistent Project Description",
            StartDate = DateTime.Today
        };

        _adminPservice.GetTasks(projectDTO);
    }

    [TestMethod]
    public void RemoveProjectLeader_ShouldRemoveAssignedLeader_WhenTheProjectrHasAProjectLeader()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Description",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPservice.CreateProject(projectDTO);

        _adminPservice.SetProjectLeader("New Project", Leader.Email);
        _adminPservice.RemoveProjectLeader("New Project");

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "New Project");
        Assert.IsNull(project.ProjectLeader);
    }

    [TestMethod]
    public void CreateProject_ShouldHandleProjectWithTasks_WhenTasksExistInDatabase()
    {
        var initialProject = new ProjectDTO
        {
            Name = "Initial Project",
            Description = "Initial project for task",
            StartDate = DateTime.Today,
            AdminProyect = Admin
        };

        _adminPservice.CreateProject(initialProject);

        var existingTask = new TaskDTO
        {
            Title = "Existing Task",
            Description = "Task Description",
            Duration = 3,
            ExpectedStartDate = DateTime.Today,
            State = StateDTO.TODO
        };

        _taskService.AddTask("Initial Project", existingTask);

        var savedTasks = _taskService.GetTasks("Initial Project");
        var savedTask = savedTasks.FirstOrDefault(t => t.Title == "Existing Task");
        Assert.IsNotNull(savedTask);

        var projectWithTasks = new ProjectDTO
        {
            Name = "Project With Tasks",
            Description = "Project with existing tasks",
            StartDate = DateTime.Today,
            AdminProyect = Admin,
            Tasks = new List<TaskDTO> { savedTask }
        };

        _adminPservice.CreateProject(projectWithTasks);

        var createdProject = _repositoryManager.ProjectRepository.Get(p => p.Name == "Project With Tasks");
        Assert.IsNotNull(createdProject);
        Assert.IsNotNull(createdProject.Tasks);
    }

    [TestMethod]
    public void CreateProject_ShouldHandleProjectWithNonExistentTasks_WhenTasksNotInDatabase()
    {
        var nonExistentTask = new TaskDTO
        {
            Id = 999,
            Title = "Non Existent Task",
            Description = "Task that doesn't exist",
            Duration = 3,
            ExpectedStartDate = DateTime.Today,
            State = StateDTO.TODO
        };

        var projectWithNonExistentTasks = new ProjectDTO
        {
            Name = "Project With Invalid Tasks",
            Description = "Project with non-existent tasks",
            StartDate = DateTime.Today,
            AdminProyect = Admin,
            Tasks = new List<TaskDTO> { nonExistentTask }
        };

        _adminPservice.CreateProject(projectWithNonExistentTasks);

        var createdProject = _repositoryManager.ProjectRepository.Get(p => p.Name == "Project With Invalid Tasks");
        Assert.IsNotNull(createdProject);
        Assert.IsNotNull(createdProject.Tasks);
        Assert.AreEqual(0, createdProject.Tasks.Count);
    }

    [TestMethod]
    public void CreateProject_ShouldHandleProjectWithExistingMembers_WhenMembersExistInDatabase()
    {
        var projectWithMembers = new ProjectDTO
        {
            Name = "Project With Existing Members",
            Description = "Project with existing members",
            StartDate = DateTime.Today,
            AdminProyect = Admin,
            Members = new List<UserDTO> { UserDTO }
        };

        _adminPservice.CreateProject(projectWithMembers);

        var createdProject = _repositoryManager.ProjectRepository.Get(p => p.Name == "Project With Existing Members");
        Assert.IsNotNull(createdProject);
        Assert.IsNotNull(createdProject.Members);
        Assert.AreEqual(1, createdProject.Members.Count);
        Assert.AreEqual("member.user@example.com", createdProject.Members[0].Email);
    }

    [TestMethod]
    public void CreateProject_ShouldHandleProjectWithNonExistentMembers_WhenMembersNotInDatabase()
    {
        var nonExistentMember = new UserDTO
        {
            FirstName = "Non",
            LastName = "Existent",
            Email = "nonexistent@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        var projectWithNonExistentMembers = new ProjectDTO
        {
            Name = "Project With Invalid Members",
            Description = "Project with non-existent members",
            StartDate = DateTime.Today,
            AdminProyect = Admin,
            Members = new List<UserDTO> { nonExistentMember }
        };

        _adminPservice.CreateProject(projectWithNonExistentMembers);

        var createdProject = _repositoryManager.ProjectRepository.Get(p => p.Name == "Project With Invalid Members");
        Assert.IsNotNull(createdProject);
        Assert.IsNotNull(createdProject.Members);
        Assert.AreEqual(0, createdProject.Members.Count);
    }

    [TestMethod]
    public void CreateProject_ShouldHandleProjectWithNullAdminProyect_WhenAdminProyectIsNull()
    {
        var projectWithNullAdmin = new ProjectDTO
        {
            Name = "Project With Null Admin",
            Description = "Project with null AdminProyect",
            StartDate = DateTime.Today,
            AdminProyect = null,
            Members = new List<UserDTO>()
        };

        _adminPservice.CreateProject(projectWithNullAdmin);

        var createdProject = _repositoryManager.ProjectRepository.Get(p => p.Name == "Project With Null Admin");
        Assert.IsNotNull(createdProject);
        Assert.IsNotNull(createdProject.AdminProject);
    }

    [TestMethod]
    public void CreateProject_ShouldHandleProjectWithNullProjectLeader_WhenProjectLeaderIsNull()
    {
        var projectWithNullLeader = new ProjectDTO
        {
            Name = "Project With Null Leader",
            Description = "Project with null ProjectLeader",
            StartDate = DateTime.Today,
            AdminProyect = Admin,
            ProjectLeader = null
        };

        _adminPservice.CreateProject(projectWithNullLeader);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "Project With Null Leader");
        Assert.IsNotNull(project);
        Assert.IsNull(project.ProjectLeader);
    }

    [TestMethod]
    public void CreateProject_ShouldHandleProjectWithExistingProjectLeader_WhenProjectLeaderExists()
    {
        var projectWithLeader = new ProjectDTO
        {
            Name = "Project With Leader",
            Description = "Project with leader",
            StartDate = DateTime.Today,
            AdminProyect = Admin,
            ProjectLeader = Leader
        };

        _adminPservice.CreateProject(projectWithLeader);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "Project With Leader");
        Assert.IsNotNull(project);
        Assert.IsNotNull(project.ProjectLeader);
        Assert.AreEqual("normal.user@example.com", project.ProjectLeader.Email);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void AssignMembersToProject_ShouldThrowException_WhenProjectNameIsNull()
    {
        _login.LoginUser("admin.user@example.com", "Password123@");

        List<UserDTO> membersToAdd = new List<UserDTO>
        {
            _userservice.GetUser("member.user@example.com")
        };

        _adminPservice.AssignMembersToProject(null, membersToAdd);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void AssignMembersToProject_ShouldThrowException_WhenProjectNameIsEmpty()
    {
        _login.LoginUser("admin.user@example.com", "Password123@");

        List<UserDTO> membersToAdd = new List<UserDTO>
        {
            _userservice.GetUser("member.user@example.com")
        };

        _adminPservice.AssignMembersToProject("", membersToAdd);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void AssignMembersToProject_ShouldThrowException_WhenMembersListIsNull()
    {
        _login.LoginUser("admin.user@example.com", "Password123@");

        var projectDTO = new ProjectDTO
        {
            Name = "Test Project for Null Members",
            Description = "Test project",
            StartDate = DateTime.Now,
            AdminProyect = Admin,
            Members = new List<UserDTO>()
        };

        _adminPservice.CreateProject(projectDTO);

        _adminPservice.AssignMembersToProject("Test Project for Null Members", null);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void AssignMembersToProject_ShouldThrowException_WhenMembersListIsEmpty()
    {
        _login.LoginUser("admin.user@example.com", "Password123@");

        var projectDTO = new ProjectDTO
        {
            Name = "Test Project for Empty Members",
            Description = "Test project",
            StartDate = DateTime.Now,
            AdminProyect = Admin,
            Members = new List<UserDTO>()
        };

        _adminPservice.CreateProject(projectDTO);

        _adminPservice.AssignMembersToProject("Test Project for Empty Members", new List<UserDTO>());
    }

    [TestMethod]
    [ExpectedException(typeof(UserIsAlreadyAMemberException))]
    public void AssignMembersToProject_ShouldThrowUserIsAlreadyAMemberException_WhenUserAlreadyMemberById()
    {
        _login.LoginUser("admin.user@example.com", "Password123@");

        var projectDTO = new ProjectDTO
        {
            Name = "Test Project Member By ID",
            Description = "Test project",
            StartDate = DateTime.Now,
            AdminProyect = Admin,
            Members = new List<UserDTO>()
        };

        _adminPservice.CreateProject(projectDTO);

        var memberToAdd = _userservice.GetUser("member.user@example.com");
        _adminPservice.AssignMembersToProject("Test Project Member By ID", new List<UserDTO> { memberToAdd });

        var sameMemberDifferentInstance = new UserDTO
        {
            FirstName = "User",
            LastName = "Member",
            Email = "member.user@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        _adminPservice.AssignMembersToProject("Test Project Member By ID",
            new List<UserDTO> { sameMemberDifferentInstance });
    }

    [TestMethod]
    public void RemoveAdminFromProject_ShouldRemoveAdmin_WhenProjectHasAdmin()
    {
        _login.LoginUser("admin.user@example.com", "Password123@");

        var projectDTO = new ProjectDTO
        {
            Name = "Project With Admin To Remove",
            Description = "Project that has an admin",
            StartDate = DateTime.Now,
            AdminProyect = Admin,
            Members = new List<UserDTO>()
        };

        _adminPservice.CreateProject(projectDTO);

        _adminPservice.RemoveAdminFromProject("Project With Admin To Remove", "admin.user@example.com");

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "Project With Admin To Remove");
        Assert.IsNull(project.AdminProject);
    }

    [TestMethod]
    [ExpectedException(typeof(ProjectNotFoundException))]
    public void RemoveAdminFromProject_ShouldThrowProjectNotFoundException_WhenProjectDoesNotExist()
    {
        _login.LoginUser("admin.user@example.com", "Password123@");

        _adminPservice.RemoveAdminFromProject("NonExistent Project", "admin.user@example.com");
    }

    [TestMethod]
    [ExpectedException(typeof(UserIsNotAMemberException))]
    public void RemoveAdminFromProject_ShouldThrowUserIsNotAMemberException_WhenProjectHasNoAdmin()
    {
        _login.LoginUser("admin.user@example.com", "Password123@");

        var projectDTO = new ProjectDTO
        {
            Name = "Project For Admin Removal",
            Description = "Project to test admin removal",
            StartDate = DateTime.Now,
            AdminProyect = Admin,
            Members = new List<UserDTO>()
        };

        _adminPservice.CreateProject(projectDTO);

        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == "Project For Admin Removal");
        project.AdminProject = null;
        _repositoryManager.ProjectRepository.Update(project);

        _adminPservice.RemoveAdminFromProject("Project For Admin Removal", "admin.user@example.com");
    }

    [TestMethod]
    [ExpectedException(typeof(UserIsNotAMemberException))]
    public void RemoveAdminFromProject_ShouldThrowUserIsNotAMemberException_WhenEmailDoesNotMatchAdmin()
    {
        _login.LoginUser("admin.user@example.com", "Password123@");

        var projectDTO = new ProjectDTO
        {
            Name = "Project With Different Admin",
            Description = "Project with specific admin",
            StartDate = DateTime.Now,
            AdminProyect = Admin,
            Members = new List<UserDTO>()
        };

        _adminPservice.CreateProject(projectDTO);

        _adminPservice.RemoveAdminFromProject("Project With Different Admin", "different.admin@example.com");
    }

    [TestMethod]
    public void GetAllProjectsForAdmin_ShouldReturnAllProjectsLoggedUserIsAdmin()
    {
        var projectDTO1 = new ProjectDTO
        {
            Name = "Project 1",
            Description = "Description 1",
            StartDate = DateTime.Now,
            Members = members
        };
        var projectDTO2 = new ProjectDTO
        {
            Name = "Project 2",
            Description = "Description 2",
            StartDate = DateTime.Now.AddDays(1),
            Members = members
        };

        _adminPservice.CreateProject(projectDTO1);
        _adminPservice.CreateProject(projectDTO2);

        var projects = _adminPservice.GetAllProjectsForAdmin();

        Assert.AreEqual(2, projects.Count);
        Assert.AreEqual("Project 1", projects[0].Name);
        Assert.AreEqual("Project 2", projects[1].Name);
    }
}