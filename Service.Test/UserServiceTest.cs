using DataAccess;
using Domain;
using Domain.Exceptions;
using Service.Models;

namespace Service.Test;

[TestClass]
public class UserServiceTest
{
    [TestMethod]
    [ExpectedException(typeof(InvalidUserEmailException))]
    public void AddUser_ShouldThrowException_WhenEmailIsNotUnique()
    {
        // Arrange
        List<Rol> rols = new List<Rol>();
        rols.Add(Rol.ProjectMember);

        UserRepository userRepository = new UserRepository();

        UserService _userService = new UserService(userRepository);

        var userDTO1 = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Roles = rols
        };

        _userService.AddUser(userDTO1);

        var userDTO2 = new UserDTO
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "john.doe@example.com", // Same email
            Password = "Password123@",
            Roles = rols
        };

        // Act & Assert
        _userService.AddUser(userDTO2); // Should throw exception
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void UpdateUser_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        var userRepository = new UserRepository();
        var userService = new UserService(userRepository);
        List<Rol> rols = new List<Rol>();
        rols.Add(Rol.ProjectMember);

        var userToUpdate = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "nonexistent.user@example.com",
            Roles = rols,
        };

        // Act & Assert
        userService.UpdateUser(userToUpdate);
    }

    [TestMethod]
    [ExpectedException(typeof(NoUsersFoundException))]
    public void GetUsers_ShouldThrowException_WhenNoUsersExist()
    {
        // Arrange
        var userRepository = new UserRepository();
        var userService = new UserService(userRepository);

        // Act & Assert
        userService.GetUsers();
    }
}
    