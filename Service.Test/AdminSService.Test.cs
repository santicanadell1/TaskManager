using DataAccess;
using Domain;
using Service.Models;

namespace Service.Test;

[TestClass]
public class AdminSService_Test
{
    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAccessException))]
    public void AdminService_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAdmin()
    {
        var database = new InMemoryDatabase();  
        var adminService = new AdminService(database);  

        var userDTO = new UserDTO
        {
            FirstName = "NonAdmin",
            LastName = "User",
            Email = "nonadmin.user@example.com",
            Password = "Password123@",
            Roles = new List<Rol> { Rol.ProjectMember }  
        };

        var loginService = new Login(database);  
        loginService.LoginUser(userDTO.Email, userDTO.Password);  

        var newUserDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Roles = new List<Rol>()  
        };

        adminService.CreateUser(newUserDTO);  
    }
}