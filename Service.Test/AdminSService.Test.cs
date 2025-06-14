using DataAccess;
using Domain;
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
    private IRepositoryManager _repositoryManager;

    [TestInitialize]
    public void TestSetUp()
    {
        InMemoryAppContextFactory contextFactory = new InMemoryAppContextFactory();
        _context = contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repositoryManager = new RepositoryManager(_context);
        _adminService = new AdminSService(_repositoryManager);
        _loginService = new Login(_repositoryManager);
        _userService = new UserService(_repositoryManager);

        UserDTO adminUserDTO = new UserDTO
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin.user@example.com",
            Password = "AdminPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.AdminSystem }
        };

        UserDTO normalUserDTO = new UserDTO
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

        var savedUser = _userService.GetUser(normalUserDTO.Email);
        Assert.IsNotNull(savedUser);
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
        UserDTO currentUser = LoggedUser.Current;

        _adminService.CreateUser(currentUser);
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void AdminService_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        UserDTO userToDeleteDTO = new UserDTO
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

        UserDTO userToUpdate = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "OldPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        string newPassword = "NewPassword456@";

        _adminService.ChangePassword(userToUpdate.Email, newPassword, userToUpdate.Password);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void AdminService_ShouldThrowUnauthorizedAccessException_WhenAssignRolAndUserIsNotAdmin()
    {
        _loginService.LoginUser("john.doe@example.com", "Password123@");

        UserDTO userToUpdate = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        RolDTO roleToAssign = RolDTO.ProjectMember;

        _adminService.AssignRole(userToUpdate, roleToAssign);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidUserPasswordException))]
    public void AdminService_ShouldThrowInvalidUserPasswordException_WhenPasswordUserIsNotInValidFormat()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        UserDTO userToUpdate = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        string newPassword = "abcD1";

        _adminService.ChangePassword(userToUpdate.Email, newPassword, "Password123@");
    }


    [TestMethod]
    public void AdminService_ShouldChangePassword_WhenChangingUserPassword()
    {
        PasswordManager _passwordManager = new PasswordManager();


        UserDTO userToUpdate = new UserDTO
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
        UserDTO user = _userService.GetUser(userToUpdate.Email);
        Assert.AreEqual(_passwordManager.HashPassword("Password123#"), user.Password);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void AdminService_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAdmin_CreateUser()
    {
        _loginService.LoginUser("john.doe@example.com", "Password123@");
        UserDTO currentUser = LoggedUser.Current;

        _adminService.CreateUser(currentUser);
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void AdminService_ShouldThrowUserNotFoundException_WhenDeletingNonExistentUser()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        UserDTO userToDeleteDTO = new UserDTO
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
    [ExpectedException(typeof(InvalidOldPasswordException))]
    public void AdminService_ShouldThrowInvalidOldPasswordException_WhenOldPasswordIsIncorrect()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        UserDTO userToUpdate = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "OldPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        string newPassword = "NewPassword456@";

        _adminService.ChangePassword(userToUpdate.Email, newPassword, "IncorrectOldPassword");
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void AdminService_ShouldThrowUnauthorizedAccessException_WhenAssigningRoleAndUserIsNotAdmin()
    {
        _loginService.LoginUser("john.doe@example.com", "Password123@");

        UserDTO userToUpdate = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        RolDTO roleToAssign = RolDTO.ProjectMember;

        _adminService.AssignRole(userToUpdate, roleToAssign);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidUserPasswordException))]
    public void AdminService_ShouldThrowInvalidUserPasswordException_WhenNewPasswordIsInvalid()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        UserDTO userToUpdate = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        string newPassword = "123";
        _adminService.ChangePassword(userToUpdate.Email, newPassword, "Password123@");
    }

    [TestMethod]
    public void AdminService_ShouldCreateUser_WhenUserIsAdmin()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        UserDTO newUserDTO = new UserDTO
        {
            FirstName = "New",
            LastName = "User",
            Email = "new.user@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1992-01-01"),
            Roles = new List<RolDTO>()
        };

        _adminService.CreateUser(newUserDTO);

        UserDTO createdUser = _userService.GetUser(newUserDTO.Email);
        Assert.IsNotNull(createdUser);
        Assert.AreEqual(newUserDTO.Email, createdUser.Email);
    }
    
    [TestMethod]
    public void AdminService_ShouldDeleteUser_WhenUserExistsAndAdminIsLoggedIn()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        UserDTO userToDelete = _userService.GetUser("john.doe@example.com");
        Assert.IsNotNull(userToDelete);

        _adminService.DeleteUser(userToDelete);

        try
        {
            UserDTO deletedUser = _userService.GetUser("john.doe@example.com");
            Assert.Fail("Expected UserNotFoundException was not thrown");
        }
        catch (UserNotFoundException)
        {

        }
    }
    
    [TestMethod]
    public void AdminService_ShouldChangePassword_WhenValidCredentialsProvided()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");
        PasswordManager passwordManager = new PasswordManager();

        string newPassword = "NewPassword123@";
        _adminService.ChangePassword("john.doe@example.com", newPassword, "Password123@");

        UserDTO updatedUser = _userService.GetUser("john.doe@example.com");
        Assert.AreEqual;
    }
    
    
    
    
    
}





