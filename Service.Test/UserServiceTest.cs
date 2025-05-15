using DataAccess;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Service.Exceptions.UserServiceExceptions;
using Service.Models;

namespace Service.Test;

[TestClass]
public class UserServiceTest
{
    [TestMethod]
    [ExpectedException(typeof(InvalidUserEmailException))]
    public void AddUser_ShouldThrowException_WhenEmailIsNotUnique()
    {
        var rols = new List<RolDTO>();
        rols.Add(RolDTO.ProjectMember);

        var userRepository = new UserRepository();

        var _userService = new UserService(new InMemoryDatabase());

        var userDTO1 = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = rols
        };

        _userService.AddUser(userDTO1);

        var userDTO2 = new UserDTO
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = rols
        };

        _userService.AddUser(userDTO2);
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void UpdateUser_ShouldThrowException_WhenUserDoesNotExist()
    {
        var userRepository = new UserRepository();
        var userService = new UserService(new InMemoryDatabase());
        var rols = new List<RolDTO>();
        rols.Add(RolDTO.ProjectMember);

        var userToUpdate = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "nonexistent.user@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = rols
        };

        userService.UpdateUser(userToUpdate);
    }

    [TestMethod]
    [ExpectedException(typeof(NoUsersFoundException))]
    public void GetUsers_ShouldThrowException_WhenNoUsersExist()
    {
        var userRepository = new UserRepository();
        var userService = new UserService(new InMemoryDatabase());

        userService.GetUsers();
    }


    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void GetUser_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
    {
        var userRepository = new UserRepository();
        var userService = new UserService(new InMemoryDatabase());

        userService.GetUser("nonexistent.user@example.com");
    }

    [TestMethod]
    public void AddUser_ShouldAddUser_WhenEmailIsUnique()
    {
        var rols = new List<RolDTO> { RolDTO.ProjectMember };
        var userDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = rols
        };

        var userService = new UserService(new InMemoryDatabase());


        userService.AddUser(userDTO);


        var users = userService.GetUsers();
        Assert.AreEqual(1, users.Count);
        Assert.AreEqual("john.doe@example.com", users[0].Email);
    }

    [TestMethod]
    public void UpdateUser_ShouldUpdateUser_WhenUserExists()
    {
        var rols = new List<RolDTO> { RolDTO.ProjectMember };
        var userDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = rols
        };

        var userService = new UserService(new InMemoryDatabase());
        userService.AddUser(userDTO);

        var updatedUserDTO = new UserDTO
        {
            FirstName = "Johnny",
            LastName = "Dough",
            Email = "john.doe@example.com",
            Password = "NewPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = rols
        };


        userService.UpdateUser(updatedUserDTO);


        var updatedUser = userService.GetUser("john.doe@example.com");
        Assert.AreEqual("Johnny", updatedUser.FirstName);
        Assert.AreEqual("Dough", updatedUser.LastName);
    }
}