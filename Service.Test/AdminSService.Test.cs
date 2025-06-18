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
    private IRepositoryManager _repositoryManager;
    private UserService _userService;

    [TestInitialize]
    public void TestSetUp()
    {
        var contextFactory = new InMemoryAppContextFactory();
        _context = contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repositoryManager = new RepositoryManager(_context);
        _adminService = new AdminSService(_repositoryManager);
        _loginService = new Login(_repositoryManager);
        _userService = new UserService(_repositoryManager);

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
        var currentUser = LoggedUser.Current;

        _adminService.CreateUser(currentUser);
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void AdminService_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

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

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void AdminService_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAdmin_CreateUser()
    {
        _loginService.LoginUser("john.doe@example.com", "Password123@");
        var currentUser = LoggedUser.Current;

        _adminService.CreateUser(currentUser);
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void AdminService_ShouldThrowUserNotFoundException_WhenDeletingNonExistentUser()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

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
    [ExpectedException(typeof(InvalidOldPasswordException))]
    public void AdminService_ShouldThrowInvalidOldPasswordException_WhenOldPasswordIsIncorrect()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

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

        _adminService.ChangePassword(userToUpdate.Email, newPassword, "IncorrectOldPassword");
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAdminAccessException))]
    public void AdminService_ShouldThrowUnauthorizedAccessException_WhenAssigningRoleAndUserIsNotAdmin()
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
    public void AdminService_ShouldThrowInvalidUserPasswordException_WhenNewPasswordIsInvalid()
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

        var newPassword = "123";
        _adminService.ChangePassword(userToUpdate.Email, newPassword, "Password123@");
    }

    [TestMethod]
    public void AdminService_ShouldCreateUser_WhenUserIsAdmin()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var newUserDTO = new UserDTO
        {
            FirstName = "New",
            LastName = "User",
            Email = "new.user@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1992-01-01"),
            Roles = new List<RolDTO>()
        };

        _adminService.CreateUser(newUserDTO);

        var createdUser = _userService.GetUser(newUserDTO.Email);
        Assert.IsNotNull(createdUser);
        Assert.AreEqual(newUserDTO.Email, createdUser.Email);
    }

    [TestMethod]
    public void AdminService_ShouldDeleteUser_WhenUserExistsAndAdminIsLoggedIn()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var userToDelete = _userService.GetUser("john.doe@example.com");
        Assert.IsNotNull(userToDelete);

        _adminService.DeleteUser(userToDelete);

        try
        {
            var deletedUser = _userService.GetUser("john.doe@example.com");
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
        var passwordManager = new PasswordManager();

        var newPassword = "NewPassword123@";
        _adminService.ChangePassword("john.doe@example.com", newPassword, "Password123@");

        var updatedUser = _userService.GetUser("john.doe@example.com");
        Assert.AreEqual(passwordManager.HashPassword(newPassword), updatedUser.Password);
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void AdminService_ShouldThrowUserNotFoundException_WhenChangingPasswordForNonExistentUser()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        _adminService.ChangePassword("nonexistent@example.com", "NewPassword123@", "OldPassword123@");
    }

    [TestMethod]
    public void AdminService_ShouldAssignRole_WhenUserExistsAndRoleNotAssigned()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var userToUpdate = _userService.GetUser("john.doe@example.com");
        var roleToAssign = RolDTO.ProjectMember;

        _adminService.AssignRole(userToUpdate, roleToAssign);

        var updatedUser = _userService.GetUser("john.doe@example.com");
        Assert.IsTrue(updatedUser.Roles.Contains(roleToAssign));
    }

    [TestMethod]
    public void AdminService_ShouldDeleteUser_WhenCurrentUserIsAlreadyAdminProject()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var currentUser = LoggedUser.Current;
        if (!currentUser.Roles.Contains(RolDTO.AdminProject)) currentUser.Roles.Add(RolDTO.AdminProject);

        var userToDelete = new UserDTO
        {
            FirstName = "Delete",
            LastName = "Me2",
            Email = "delete.me2@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        _userService.AddUser(userToDelete);

        _adminService.DeleteUser(userToDelete);

        try
        {
            var deletedUser = _userService.GetUser("delete.me2@example.com");
            Assert.Fail("Expected UserNotFoundException was not thrown");
        }
        catch (UserNotFoundException)
        {
        }
    }

    [TestMethod]
    public void AdminService_ShouldDeleteUser_AndHandleProjectRemovalExceptions()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var userToDelete = new UserDTO
        {
            FirstName = "Delete",
            LastName = "Me3",
            Email = "delete.me3@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        _userService.AddUser(userToDelete);

        _adminService.DeleteUser(userToDelete);

        try
        {
            var deletedUser = _userService.GetUser("delete.me3@example.com");
            Assert.Fail("Expected UserNotFoundException was not thrown");
        }
        catch (UserNotFoundException)
        {
        }
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOldPasswordException))]
    public void AdminService_ShouldThrowInvalidOldPasswordException_WhenCurrentUserOldPasswordIsWrong()
    {
        _loginService.LoginUser("john.doe@example.com", "Password123@");

        _adminService.ChangeCurrentUserPassword("john.doe@example.com", "WrongOldPassword123@", "NewPassword123@");
    }

    [TestMethod]
    public void AdminService_ShouldDeleteUserFromRepository_WhenUserExists()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var userToDelete = new UserDTO
        {
            FirstName = "Repository",
            LastName = "Test",
            Email = "repository.test@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO>()
        };

        _userService.AddUser(userToDelete);

        var userEntityBeforeDelete =
            _repositoryManager.UserRepository.Get(u => u.Email == "repository.test@example.com");
        Assert.IsNotNull(userEntityBeforeDelete);

        _adminService.DeleteUser(userToDelete);

        var userEntityAfterDelete =
            _repositoryManager.UserRepository.Get(u => u.Email == "repository.test@example.com");
        Assert.IsNull(userEntityAfterDelete);
    }

    [TestMethod]
    public void AdminService_ShouldRemoveRole_WhenUserExistsAndRoleIsAssigned()
    {
        _loginService.LoginUser("admin.user@example.com", "AdminPassword123@");

        var userToUpdate = _userService.GetUser("john.doe@example.com");
        var roleToAssign = RolDTO.ProjectMember;

        _adminService.AssignRole(userToUpdate, roleToAssign);

        var updatedUser = _userService.GetUser("john.doe@example.com");
        Assert.IsTrue(updatedUser.Roles.Contains(roleToAssign));

        _adminService.RemoveRole(userToUpdate, roleToAssign);
        updatedUser = _userService.GetUser("john.doe@example.com");
        Assert.IsFalse(updatedUser.Roles.Contains(roleToAssign));
    }
}