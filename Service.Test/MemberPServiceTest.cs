using DataAccess;
using Domain;
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

    [TestInitialize]
    public void Initialize()
    {
        database = new InMemoryDatabase();
        _memberPService = new MemberPService(database);
        _taskService = new TaskService(database);
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
            Roles = new List<Rol> { Rol.AdminProject }
        };

        UserDTO = new UserDTO
        {
            FirstName = "User",
            LastName = "Member",
            Email = "member.user@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<Rol> { Rol.ProjectMember }
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
        TaskDTO task = new TaskDTO()
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

   
}