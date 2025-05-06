using DataAccess;
using Domain;
using Domain.Exceptions;
using Service.Models;

namespace Service.Test;

[TestClass]
public class AdminSService_Test
{
    private InMemoryDatabase _database;
    private AdminSService _adminService;
    private Login _loginService;
    private UserService _userService;

    [TestInitialize]
    public void TestSetUp()
    {
        _database = new InMemoryDatabase();
        _adminService = new AdminSService(_database);
        _loginService = new Login(_database);
        _userService = new UserService(_database);

        var adminUserDTO = new UserDTO
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin.user@example.com",
            Password = "AdminPassword123@",
            Roles = new List<Rol> { Rol.AdminSystem }
        };

        var normalUserDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Roles = new List<Rol>()
        };

        _userService.AddUser(normalUserDTO);
        _userService.AddUser(adminUserDTO);
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
            Roles = new List<Rol>()
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
            Roles = new List<Rol>()
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
            Roles = new List<Rol>()
        };

        var roleToAssign = Rol.ProjectMember;

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
            Roles = new List<Rol>()
        };

        var newPassword = "abcD1";

        _adminService.ChangePassword(userToUpdate.Email, newPassword, "Password123@");
    }
}