using DataAccess;
using Domain.Exceptions.TaskExceptions;
using Service.Exceptions.MemberServiceExceptions;
using Service.Models;

namespace Service.Test;

[TestClass]
public class MemberPServiceTest
{
    private AdminPService _adminPService;
    private AppDbContext _context;
    private InMemoryAppContextFactory _contextFactory;
    private Login _login;
    private MemberPService _memberPService;
    private IRepositoryManager _repositoryManager;
    private TaskService _taskService;
    private UserService _userservice;
    private UserDTO Admin;
    private List<UserDTO> members;
    private ProjectDTO projectDTO;

    private TaskDTO task1;
    private TaskDTO task2;
    private TaskDTO task3;
    private UserDTO UserDTO;

    [TestInitialize]
    public void Initialize()
    {
        _contextFactory = new InMemoryAppContextFactory();
        _context = _contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repositoryManager = new RepositoryManager(_context);

        _memberPService = new MemberPService(_repositoryManager);

        var cpmService = new CpmService();

        _taskService = new TaskService(_repositoryManager, cpmService);
        _adminPService = new AdminPService(_repositoryManager);
        _login = new Login(_repositoryManager);
        _userservice = new UserService(_repositoryManager);

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
            Roles = new List<RolDTO> { RolDTO.ProjectMember, RolDTO.AdminProject }
        };

        members = new List<UserDTO> { UserDTO };

        _userservice.AddUser(Admin);
        _userservice.AddUser(UserDTO);
        _login.LoginUser(Admin.Email, Admin.Password);

        projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };

        task1 = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today,
            State = StateDTO.TODO
        };

        task2 = new TaskDTO
        {
            Title = "Task2",
            Description = "Description2",
            Duration = 1,
            ExpectedStartDate = DateTime.Today,
            State = StateDTO.TODO
        };

        task3 = new TaskDTO
        {
            Title = "Task 3",
            Description = "Description 3",
            Duration = 1,
            ExpectedStartDate = DateTime.Parse("2026-01-01"),
            PreviousTasks = new List<TaskDTO> { task1 }
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Database.EnsureDeleted();
    }

    [TestMethod]
    public void GetAllProjectsForMember_WhenMemberIsAssigned_ThenReturnProjects()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Project 1",
            Description = "Project Description",
            StartDate = DateTime.Today,
            Members = members
        };

        _adminPService.CreateProject(projectDTO);

        var result = _memberPService.GetAllProjectsFromAMember(UserDTO.Email);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(projectDTO.Name, result[0].Name);
    }

    [TestMethod]
    [ExpectedException(typeof(UserHasNoProjectsException))]
    public void GetAllProjectsForMember_WhenUserHasNoProjectMemberRole_ThenThrowPermissionException()
    {
        var user = new UserDTO
        {
            FirstName = "NoRole",
            LastName = "User",
            Email = "no.role@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<RolDTO> { RolDTO.AdminSystem }
        };

        _userservice.AddUser(user);

        _memberPService.GetAllProjectsFromAMember(user.Email);
    }

    [TestMethod]
    [ExpectedException(typeof(UserHasNoProjectsException))]
    public void GetAllProjectsForMember_WhenUserHasNoProjects_ThenThrowNoProjectsException()
    {
        var user = new UserDTO
        {
            FirstName = "Another",
            LastName = "Member",
            Email = "another.member@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        _userservice.AddUser(user);

        _memberPService.GetAllProjectsFromAMember(user.Email);
    }

    [TestMethod]
    public void GetAllProjectsForMember_WhenUserHasMultipleProjects_ThenReturnAllProjects()
    {
        var projectDTO1 = new ProjectDTO
        {
            Name = "Project 1",
            Description = "Project Description",
            StartDate = DateTime.Today,
            Members = members
        };

        _adminPService.CreateProject(projectDTO1);

        var projectDTO2 = new ProjectDTO
        {
            Name = "Project 2",
            Description = "Another project",
            StartDate = DateTime.Today,
            Members = new List<UserDTO> { UserDTO }
        };

        _adminPService.CreateProject(projectDTO2);

        var result = _memberPService.GetAllProjectsFromAMember(UserDTO.Email);

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(p => p.Name == projectDTO1.Name));
        Assert.IsTrue(result.Any(p => p.Name == projectDTO2.Name));
    }

    [TestMethod]
    public void ChangeTaskStatus_WhenUserIsMember_ThenStateIsUpdated()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Project 3",
            Description = "Project Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPService.CreateProject(projectDTO);

        _taskService.AddTask(projectDTO.Name, task1);
        var newState = StateDTO.DONE;
        var newTask = _taskService.GetTasks(projectDTO.Name).Find(t => t.Title == task1.Title);
        _adminPService.AddTaskToMember(projectDTO.Name, UserDTO.Email, newTask.Title);
        _memberPService.ChangeTaskStatus(projectDTO.Name, UserDTO.Email, newTask, newState);

        var updatedTask = _taskService.GetTask(projectDTO.Name, task1.Title);
        Assert.AreEqual(StateDTO.DONE, updatedTask.State);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskCantBeModifiedByUserException))]
    public void ChangeTaskStatus_WhentaskIsNotFromAMember_ThenThrowException()
    {
        _adminPService.CreateProject(projectDTO);
        _taskService.AddTask(projectDTO.Name, task1);
        var User = new UserDTO
        {
            FirstName = "User",
            LastName = "NotMember",
            Email = "User.NotMember@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<RolDTO>()
        };
        _userservice.AddUser(User);

        var task = _taskService.GetTasks("New Project").First();
        var newState = StateDTO.DOING;

        _memberPService.ChangeTaskStatus("New Project", User.Email, task, newState);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskException))]
    public void ChangeTaskStatus_ShouldThrowException_WhenPreviousTasksAreNotFinished()
    {
        _adminPService.CreateProject(projectDTO);
        _taskService.AddTask("New Project", task1);

        var addedTask1 = _taskService.GetTask("New Project", task1.Title);

        var task3WithDependency = new TaskDTO
        {
            Title = "Task 3",
            Description = "Description 3",
            Duration = 1,
            ExpectedStartDate = DateTime.Parse("2026-01-01"),
            PreviousTasks = new List<TaskDTO> { addedTask1 }
        };

        _taskService.AddTask("New Project", task3WithDependency);

        var newState = StateDTO.DONE;
        var newTask = _taskService.GetTasks("New Project").Find(t => t.Title == task3WithDependency.Title);
        _adminPService.AddTaskToMember("New Project", UserDTO.Email, newTask.Title);
        _memberPService.ChangeTaskStatus("New Project", UserDTO.Email, newTask, newState);
    }

    [TestMethod]
    public void ChangeTaskStatus_ShouldChangeState_WhenPreviousTasksAreFinished()
    {
        _adminPService.CreateProject(projectDTO);
        _taskService.AddTask(projectDTO.Name, task1);
        var newState = StateDTO.DONE;
        var newTask = _taskService.GetTasks(projectDTO.Name).Find(t => t.Title == task1.Title);
        _adminPService.AddTaskToMember(projectDTO.Name, UserDTO.Email, newTask.Title);
        _memberPService.ChangeTaskStatus(projectDTO.Name, UserDTO.Email, newTask, newState);

        _taskService.AddTask("New Project", task3);
        newTask = _taskService.GetTask("New Project", task3.Title);

        _adminPService.AddTaskToMember("New Project", UserDTO.Email, newTask.Title);
        Assert.AreEqual(newTask.Id.Value, _taskService.GetTask("New Project", newTask.Title).Id.Value);

        _memberPService.ChangeTaskStatus("New Project", UserDTO.Email, newTask, newState);

        var updatedTask = _taskService.GetTask("New Project", newTask.Title);
        Assert.AreEqual(StateDTO.DONE, updatedTask.State);
    }
}