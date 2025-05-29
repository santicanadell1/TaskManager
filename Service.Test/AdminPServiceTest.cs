using DataAccess;
using DataAccess.Exceptions.ProjectRepositoryExceptions;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Models;

namespace Service.Test;

[TestClass]
public class AdminPServiceTests
{
    private AppDbContext _database;
    private Login _login;
    private AdminPService _service;
    private TaskService _taskService;
    private UserService _userservice;
    private UserDTO Admin;
    private List<UserDTO> members;
    private UserDTO UserDTO;
    private UserRepository _userRepository;
    private ProjectRepository _projectRepository;
    private NotificationRepository _notificationRepository;


    [TestInitialize]
    public void Setup()
    {
        _service = new AdminPService(_userRepository,_projectRepository, _notificationRepository);
        _userservice = new UserService(_userRepository);
        _login = new Login(_userRepository);
        _taskService = new TaskService(_projectRepository,_notificationRepository,_userRepository, new CpmService());

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

        members = new List<UserDTO> { UserDTO };

        _userservice.AddUser(Admin);
        _userservice.AddUser(UserDTO);
        _login.LoginUser(Admin.Email, Admin.Password);
    }

    [TestMethod]
    public void CreateProject_ShouldAddProjectToDatabase_WhenValid()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };

        _service.CreateProject(projectDTO);

        var project = _projectRepository.GetProject(p => p.Name == projectDTO.Name);
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

        _service.CreateProject(projectDTO);

        var project = _projectRepository.GetProject(p => p.Name == projectDTO.Name);

        var userDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "password123",
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };

        _service.AssignMembersToProject(project.Name, new List<UserDTO> { userDTO });

        Assert.IsTrue(project.Members.Count > 0);
        Assert.AreEqual("John", project.Members[1].FirstName);
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

        _service.CreateProject(projectDTO);

        var project = _projectRepository.GetProject(p => p.Name == projectDTO.Name);

        var userDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "password123",
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };

        _service.AssignMembersToProject(project.Name, new List<UserDTO> { userDTO });
        _service.AssignMembersToProject(project.Name, new List<UserDTO> { userDTO });

        Assert.IsTrue(project.Members.Count > 0);
        Assert.AreEqual("John", project.Members[1].FirstName);
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
            Members = members
        };

        _service.CreateProject(projectDTO);

        var project = _projectRepository.GetProject(p => p.Name == projectDTO.Name);

        var userDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "password123",
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };

        _service.AssignMembersToProject(project.Name, new List<UserDTO> { userDTO });

        Assert.IsTrue(project.Members.Count > 0);
        Assert.AreEqual("John", project.Members[1].FirstName);

        _service.RemoveMemberFromProject(project.Name, "john.doe@example.com");
        _service.RemoveMemberFromProject(project.Name, "member.user@example.com");

        project = _projectRepository.GetProject(p => p.Name == projectDTO.Name);

        Assert.IsTrue(project.Members.Count == 0);
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

        _service.CreateProject(projectDTO);

        var project = _projectRepository.GetProject(p => p.Name == projectDTO.Name);

        _service.RemoveMemberFromProject("Proyecto 1", "member.user@example.com");
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

        _service.CreateProject(projectDTO);

        var project = _projectRepository.GetProject(p => p.Name == projectDTO.Name);

        _service.RemoveMemberFromProject(project.Name, "john.user@example.com");
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

        _service.CreateProject(projectDTO);

        var project = _projectRepository.GetProject(p => p.Name == projectDTO.Name);
        Assert.IsNotNull(project);

        _service.RemoveProject("New Project");

        project = _projectRepository.GetProject(p => p.Name == "New Project");
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

        _service.CreateProject(projectDTO);

        var project = _projectRepository.GetProject(p => p.Name == projectDTO.Name);
        Assert.IsNotNull(project);

        var updatedDTO = new ProjectDTO
        {
            Name = "Updated Project",
            Description = "Updated Description",
            StartDate = DateTime.Now.AddDays(1),
            AdminProyect = UserDTO,
            Members = members
        };

        _service.UpdateProject("Old Project", updatedDTO);
    }

    [TestMethod]
    public void GetAllProjects_ShouldReturnAllProjects()
    {
        var projectDTO1 = new ProjectDTO
        {
            Name = "Project 1",
            Description = "Description 1",
            StartDate = DateTime.Now,
            AdminProyect = UserDTO,
            Members = members
        };
        var projectDTO2 = new ProjectDTO
        {
            Name = "Project 2",
            Description = "Description 2",
            StartDate = DateTime.Now.AddDays(1),
            AdminProyect = UserDTO,
            Members = members
        };

        _service.CreateProject(projectDTO1);
        _service.CreateProject(projectDTO2);

        var projects = _service.GetAllProjects();

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

        _service.CreateProject(projectDTO);

        var project = _service.GetProjectByName("Test Project");

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

        _service.CreateProject(projectDTO);

        var project = _service.GetAllProjectsForUser(UserDTO.Email);

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
        _service.CreateProject(projectDTO);
        var projectMembers = _service.GetMembers("Test Project");
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
        _service.CreateProject(projectDTO);
        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        _taskService.AddTask("Test Project", task);

        _service.AddTaskToMember("Test Project", "member.user@example.com", 1);

        Assert.IsTrue(_userservice.GetUser("member.user@example.com").Tasks.Contains(1));
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
        _service.CreateProject(projectDTO);
        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        _taskService.AddTask("Test Project", task);

        _service.AddTaskToMember("Test Project", "member1.user@example.com", 1);
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
        _service.CreateProject(projectDTO);
        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        _taskService.AddTask("Test Project", task);

        _service.AddTaskToMember("Test Project", "member.user@example.com", 2);
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
        _service.CreateProject(projectDTO);
        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        _taskService.AddTask("Test Project", task);

        _service.AddTaskToMember("Test Project", "member.user@example.com", 1);

        Assert.IsTrue(_userservice.GetUser("member.user@example.com").Tasks.Contains(1));

        _service.RemoveTaskFromMember("Test Project", "member.user@example.com", 1);

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
        _service.CreateProject(projectDTO);
        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        _taskService.AddTask("Test Project", task);

        _service.AddTaskToMember("Test Project", "member.user@example.com", 1);
        _service.RemoveTaskFromMember("Test Project", "member1.user@example.com", 1);
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
        _service.CreateProject(projectDTO);
        var task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        _taskService.AddTask("Test Project", task);

        _service.AddTaskToMember("Test Project", "member.user@example.com", 1);
        _service.RemoveTaskFromMember("Test Project", "member.user@example.com", 2);
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
        _service.CreateProject(projectDTO);
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

        _service.AddTaskToMember("Test Project", "member.user@example.com", 1);
        _service.AddTaskToMember("Test Project", "member.user@example.com", 2);

        var tasks = _service.GetAllTaskForAMember("member.user@example.com");

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
        _service.CreateProject(projectDTO);
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

        _service.AddTaskToMember("Test Project", "member.user@example.com", 1);
        _service.AddTaskToMember("Test Project", "member.user@example.com", 2);

        var tasks = _service.GetAllTaskForAMemberInAProject("Test Project", "member.user@example.com");

        Assert.AreEqual(2, tasks.Count);
    }
}