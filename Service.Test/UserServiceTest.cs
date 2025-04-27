using DataAccess;
using Domain;
using Service.Models;
using DataAccess;

namespace Service.Test;

[TestClass]
public class UserServiceTest
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void AddUser_ShouldThrowException_WhenEmailIsNotUnique()
    {
        
       
        // Arrange
        List<Rol> rols = new List<Rol>();
        rols.Add(Rol.ProjectMember);
        
        UserRepository userRepository = new UserRepository(new DataAccess.DataAccess());
        
        UserService _userService = new UserService(userRepository);
        
        var userDTO1 = new UserDTO
        {
            FirstName =  "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Roles = rols
        };
        
        _userService.AddUser(userDTO1);

        var userDTO2 = new UserDTO
        {
            FirstName =  "Jane",
            LastName = "Doe",
            Email = "john.doe@example.com", // Same email
            Password = "Password123@",
            Roles = rols
        };

        // Act & Assert
        _userService.AddUser(userDTO2); // Should throw exception
    }
}