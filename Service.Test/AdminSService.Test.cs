using DataAccess;
using Service.Exceptions.AdminSServiceExceptions;
using Service.Exceptions.UserServiceExceptions;
using Service.Models;
using UserNotFoundException = DataAccess.Exceptions.UserRepositoryExceptions.UserNotFoundException;

namespace Service.Test;

[TestClass]
public class AdminSService_Test
{
    private AdminSService _adminService;
    private AppDbContext _context;
    private Login _loginService;
    private UserService _userService;
    private UserRepository _userRepository;
    private ProjectRepository _projectRepository;
    private TaskRepository _taskRepository;
    private ResourceRepository _resourceRepository;


    [TestInitialize]
    public void TestSetUp()
    {
        var contextFactory = new InMemoryAppContextFactory();
        _context = contextFactory.CreateDbContext();
        
        _userRepository = new UserRepository(_context);
        _projectRepository = new ProjectRepository(_context);
        _taskRepository = new TaskRepository(_context);
        _resourceRepository = new ResourceRepository(_context);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        
        _adminService = new AdminSService(_userRepository,_projectRepository,_taskRepository,_resourceRepository);
        _loginService = new Login(_userRepository);
        _userService = new UserService(_userRepository);

        var adminUserDTO = new UserDTO
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin.user@example.com",
            Password = "AdminPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.AdminSystem }
        };

        var normalUserDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };
        _userService.AddUser(adminUserDTO);
        _userService.AddUser(normalUserDTO);
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context?.Database.EnsureDeleted();
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void AdminService_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAdmin()
    {
        _loginService.LoginUser("john.doe@example.com", "Password123@");
        var currentUser = LoggedUser.Current;

        _adminService.CreateUser(currentUser);
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void AdminService_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");
        var currentUser = LoggedUser.Current;

        var userToDeleteDTO = new UserDTO
        {
            FirstName = "Nonexistent",
            LastName = "User",
            Email = "nonexistent.user@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        _adminService.DeleteUser(userToDeleteDTO);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void AdminService_ShouldThrowUnauthorizedAccessException_WhenChangePasswordUserIsNotAdmin()
    {
        _loginService.LoginUser("john.doe@example.com", "Password123@");

        var userToUpdate = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "OldPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        var newPassword = "NewPassword456@";

        _adminService.ChangePassword(userToUpdate.Email, newPassword, userToUpdate.Password);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void AdminService_ShouldThrowUnauthorizedAccessException_WhenAssignRolAndUserIsNotAdmin()
    {
        _loginService.LoginUser("john.doe@example.com", "Password123@");

        var userToUpdate = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        var roleToAssign = RolDTO.ProjectMember;

        _adminService.AssignRole(userToUpdate, roleToAssign);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidUserPasswordException))]
    public void AdminService_ShouldThrowInvalidUserPasswordException_WhenPasswordUserIsNotInValidFormat()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var userToUpdate = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        var newPassword = "abcD1";

        _adminService.ChangePassword(userToUpdate.Email, newPassword, "Password123@");
    }

    [TestMethod]
    public void AdminService_ShouldChangePassword_WhenChangingToDefaultPassword()
    {
        var _passwordManager = new PasswordManager();
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var userToUpdate = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        _adminService.ChangeToDefaultPassword(userToUpdate.Email, "Password123@");
        var user = _userService.GetUser(userToUpdate.Email);
        Assert.AreEqual(_passwordManager.HashPassword("Password123#"), user.Password);
    }

    [TestMethod]
    public void AdminService_ShouldChangePassword_WhenChangingUserPassword()
    {
        var _passwordManager = new PasswordManager();


        var userToUpdate = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };
        _loginService.LoginUser("john.doe@example.com", "Password123@");
        _adminService.ChangeCurrentUserPassword(userToUpdate.Email, "Password123@", "Password123#");
        var user = _userService.GetUser(userToUpdate.Email);
        Assert.AreEqual(_passwordManager.HashPassword("Password123#"), user.Password);
    }
}