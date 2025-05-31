using DataAccess;
using Domain.Exceptions.TaskExceptions;
using Service.Exceptions.AdminPServiceExceptions;
using Service.Exceptions.MemberServiceExceptions;
using Service.Models;

namespace Service.Test;

[TestClass]
public class MemberPServiceTest
{
    private AdminPService _adminPService;
    private Login _login;
    private MemberPService _memberPService;
    private TaskService _taskService;
    private UserService _userservice;
    private UserDTO Admin;
    private AppDbContext _context;
    private List<UserDTO> members;
    private TaskDTO task;
    private UserDTO UserDTO;
    private UserRepository _userRepository;
    private ProjectRepository _projectRepository;
    private NotificationRepository _notificationRepository;
    private TaskRepository _taskRepository;
    private ResourceRepository _resourceRepository;
    

    [TestInitialize]
    public void Initialize()
    {
        var contextFactory = new InMemoryAppContextFactory();
        _context = contextFactory.CreateDbContext();
        
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        
        _userRepository = new UserRepository(_context);
        _projectRepository = new ProjectRepository(_context);
        _notificationRepository = new NotificationRepository(_context);
        _taskRepository = new TaskRepository(_context);
        _resourceRepository = new ResourceRepository(_context);
        
        _memberPService = new MemberPService(_userRepository, _projectRepository, _notificationRepository, _taskRepository, _resourceRepository);
        
        CpmService cpmService = new CpmService();
        
        _taskService = new TaskService(_projectRepository,_notificationRepository,_userRepository,cpmService, _taskRepository, _resourceRepository);
        _adminPService = new AdminPService(_userRepository, _projectRepository, _notificationRepository, _taskRepository, _resourceRepository);
        _login = new Login(_userRepository);
        _userservice = new UserService(_userRepository);
        
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
            Roles = new List<RolDTO> { RolDTO.ProjectMember,RolDTO.AdminProject }
        };

        members = new List<UserDTO> { UserDTO };

        _userservice.AddUser(Admin);
        _userservice.AddUser(UserDTO);
        _login.LoginUser(Admin.Email, Admin.Password);

        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };
        
        _adminPService.CreateProject(projectDTO);
        
        task = new TaskDTO
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today,
            State = StateDTO.DOING
        };
        
        var task2 = new TaskDTO
        {
            Title = "Task2",
            Description = "Description2",
            Duration = 1,
            ExpectedStartDate = DateTime.Today
        };
        
        _taskService.AddTask("New Project", task);
        _taskService.AddTask("New Project", task2);
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _context?.Database.EnsureDeleted();
    }
    
    [TestMethod]
    public void GetAllProjectsForMember_WhenMemberIsAssigned_ThenReturnProjects()
    {
        var result = _memberPService.GetAllProjectsFromAMember(UserDTO.Email);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("New Project", result[0].Name);
    }

    [TestMethod]
    [ExpectedException(typeof(UserIsNotAMemberException))]
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
        var secondProject = new ProjectDTO
        {
            Name = "Second Project",
            Description = "Another project",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = new List<UserDTO> { UserDTO }
        };

        _adminPService.CreateProject(secondProject);

        var result = _memberPService.GetAllProjectsFromAMember(UserDTO.Email);

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(p => p.Name == "New Project"));
        Assert.IsTrue(result.Any(p => p.Name == "Second Project"));
    }

    [TestMethod]
    public void ChangeTaskStatus_WhenUserIsMember_ThenStateIsUpdated()
    {
        var newState = StateDTO.DONE;
        task = _taskService.GetTask("New Project", 1);
        _adminPService.AddTaskToMember("New Project", UserDTO.Email, (int)task.Id);
        _memberPService.ChangeTaskStatus("New Project", UserDTO.Email, task, newState);
        var updatedTask = _taskService.GetTask("New Project", 1);

        Assert.AreEqual(StateDTO.DONE, updatedTask.State);
    }

    [TestMethod]
    [ExpectedException(typeof(TaskCantBeModifiedByUserException))]
    public void ChangeTaskStatus_WhentaskIsNotFromAMember_ThenThrowException()
    {
        var User = new UserDTO
        {
            FirstName = "User",
            LastName = "NotMember",
            Email = "User.NotMember@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
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
        var task = _taskService.GetTasks("New Project").First();
        var task3 = new TaskDTO
        {
            Title = "Task 3",
            Description = "Description 3",
            Duration = 1,
            ExpectedStartDate = DateTime.Today,
            PreviousTasks = new List<TaskDTO> { task }
        };
        _taskService.AddTask("New Project", task3);

        var adminPService = new AdminPService(_userRepository, _projectRepository, _notificationRepository, _taskRepository, _resourceRepository);;
        var newState = StateDTO.DONE;
        var newTask = _taskService.GetTasks("New Project").Find(t => t.Title == "Task 3");
        adminPService.AddTaskToMember("New Project", UserDTO.Email, (int)newTask.Id);
        _memberPService.ChangeTaskStatus("New Project", UserDTO.Email, newTask, newState);
    }

    [TestMethod]
    public void ChangeTaskStatus_ShouldChangeState_WhenPreviousTasksAreFinished()
    {
        var task = _taskService.GetTasks("New Project").First();
        _adminPService.AddTaskToMember("New Project", UserDTO.Email, (int)task.Id);
        _memberPService.ChangeTaskStatus("New Project", UserDTO.Email, task, StateDTO.DONE);
        
        var task3 = new TaskDTO
        {
            Title = "Task 3",
            Description = "Description 3",
            Duration = 1,
            ExpectedStartDate = DateTime.Today,
            PreviousTasks = new List<TaskDTO> { task }
        };
        
        _taskService.AddTask("New Project", task3);


        var newState = StateDTO.DONE;
        var newTask = _taskService.GetTasks("New Project").Find(t => t.Title == "Task 3");
        _adminPService.AddTaskToMember("New Project", UserDTO.Email, (int)newTask.Id);
        _memberPService.ChangeTaskStatus("New Project", UserDTO.Email, newTask, newState);
        var updatedTask = _taskService.GetTask("New Project", 3);
        Assert.AreEqual(StateDTO.DONE, updatedTask.State);
    }
}