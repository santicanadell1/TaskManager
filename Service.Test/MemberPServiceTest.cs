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
    private UserDTO UserDTO;
    private UserRepository _userRepository;
    private ProjectRepository _projectRepository;
    private NotificationRepository _notificationRepository;
    private TaskRepository _taskRepository;
    private ResourceRepository _resourceRepository;


    private TaskDTO task1;
    private TaskDTO task2;
    private TaskDTO task3;
    private ProjectDTO projectDTO;

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

        _memberPService = new MemberPService(_userRepository, _projectRepository, _notificationRepository,
            _taskRepository, _resourceRepository);

        CpmService cpmService = new CpmService();

        _taskService = new TaskService(_projectRepository, _notificationRepository, _userRepository, cpmService,
            _taskRepository, _resourceRepository);
        _adminPService = new AdminPService(_userRepository, _projectRepository, _notificationRepository,
            _taskRepository, _resourceRepository);
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
            AdminProyect = UserDTO,
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
            Name = "Project 1", // Usamos nombre único para evitar conflictos
            Description = "Project Description",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
            Members = members
        };

        _adminPService.CreateProject(projectDTO1);

        var projectDTO2 = new ProjectDTO
        {
            Name = "Project 2", // Nombre único
            Description = "Another project",
            StartDate = DateTime.Today,
            AdminProyect = UserDTO,
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
            Roles = new List<RolDTO> {}
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
        _taskService.AddTask("New Project", task3);

        var newState = StateDTO.DONE;
        var newTask = _taskService.GetTasks("New Project").Find(t => t.Title == task3.Title);
        _adminPService.AddTaskToMember("New Project", UserDTO.Email, newTask.Title);
        _memberPService.ChangeTaskStatus("New Project", UserDTO.Email, newTask, newState);
    }

    [TestMethod]
    public void ChangeTaskStatus_ShouldChangeState_WhenPreviousTasksAreFinished()
    {
        _adminPService.CreateProject(projectDTO);
        _taskService.AddTask("New Project", task1);
        var task = _taskService.GetTask("New Project", task1.Title);
        _adminPService.AddTaskToMember("New Project", UserDTO.Email, task.Title);
        _memberPService.ChangeTaskStatus("New Project", UserDTO.Email, task1, StateDTO.DONE);
        _taskService.AddTask("New Project", task3);
        var newTask = _taskService.GetTask("New Project", task3.Title); 

        var newState = StateDTO.DONE;
        _adminPService.AddTaskToMember("New Project", UserDTO.Email, newTask.Title);
        Assert.AreEqual(newTask.Id.Value,_taskService.GetTask("New Project", newTask.Title).Id.Value);;
        _memberPService.ChangeTaskStatus("New Project", UserDTO.Email, newTask, newState);

        // Recuperar la tarea actualizada y verificar su estado
        var updatedTask = _taskService.GetTask("New Project", newTask.Title); // Usar newTask.Id para recuperar la tarea actualizada
        Assert.AreEqual(StateDTO.DONE, updatedTask.State);
    }
}