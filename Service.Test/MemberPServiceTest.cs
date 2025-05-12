using DataAccess;
using Domain;
using Domain.Exceptions;
using Domain.Exceptions.TaskRepositoryExceptions;
using Service.MemberServiceException;
using Service.Models;

namespace Service.Test;

[TestClass]
public class MemberPServiceTest
{
    private InMemoryDatabase database;
    private MemberPService _memberPService;
    private TaskService _taskService;
    private AdminPService _adminPService;
    private UserDTO UserDTO;
    private UserDTO Admin;
    private List<UserDTO> members;
    private Login _login;
    private UserService _userservice;
    private TaskDTO task;

    [TestInitialize]
    public void Initialize()
    {
        database = new InMemoryDatabase();
        _memberPService = new MemberPService(database);
        _taskService = new TaskService(database, new CpmService()); 
        _adminPService = new AdminPService(database);
        _login = new Login(database);
        _userservice = new UserService(database);
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

        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Parse("2021-09-01"),
            AdminProyect = UserDTO,
            Members = members
        };
        _adminPService.CreateProject(projectDTO);
        task = new TaskDTO()
        {
            Title = "Task1",
            Description = "Description",
            Duration = 1,
            ExpectedStartDate = DateTime.Today,
        };
        TaskDTO task2 = new TaskDTO()
        {
            Title = "Task2",
            Description = "Description2",
            Duration = 1,
            ExpectedStartDate = DateTime.Today,
        };
        _taskService.AddTask("New Project", task);
        _taskService.AddTask("New Project", task2);
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
            StartDate = DateTime.Parse("2022-01-01"),
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
        AdminPService adminPService = new AdminPService(database);
        StateDTO newState = StateDTO.DONE;
        task = _taskService.GetTask("New Project", 1);
        adminPService.AddTaskToMember("New Project", UserDTO.Email, (int)task.Id);
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
    

}