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
}
