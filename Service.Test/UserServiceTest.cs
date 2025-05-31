using DataAccess;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Service.Exceptions.UserServiceExceptions;
using Service.Models;

namespace Service.Test;

[TestClass]
public class UserServiceTest
{
    private AppDbContext _context;
    private Login _loginService;
    private UserService _userService;
    
    private UserRepository _userRepository;
    
    
    [TestInitialize]
    public void TestSetUp()
    {
        var contextFactory = new InMemoryAppContextFactory();
        var _context = contextFactory.CreateDbContext();
        
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        
        _userRepository = new UserRepository(_context);
        
        _userService = new UserService(_userRepository);
        _loginService = new Login(_userRepository);
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context?.Database.EnsureDeleted();
    }
    
    [TestMethod]
    [ExpectedException(typeof(InvalidUserEmailException))]
    public void AddUser_ShouldThrowException_WhenEmailIsNotUnique()
    {
        var rols = new List<RolDTO>();
        rols.Add(RolDTO.ProjectMember);
        
        var _userService = new UserService(_userRepository);

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

        _userService.UpdateUser(userToUpdate);
    }

    [TestMethod]
    [ExpectedException(typeof(NoUsersFoundException))]
    public void GetUsers_ShouldThrowException_WhenNoUsersExist()
    {
        _userService.GetUsers();
    }


    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void GetUser_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
    {
        _userService.GetUser("nonexistent.user@example.com");
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

        _userService.AddUser(userDTO);


        var users = _userService.GetUsers();
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

        _userService.AddUser(userDTO);

        var updatedUserDTO = new UserDTO
        {
            FirstName = "Johnny",
            LastName = "Dough",
            Email = "john.doe@example.com",
            Password = "NewPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = rols
        };


        _userService.UpdateUser(updatedUserDTO);


        var updatedUser = _userService.GetUser("john.doe@example.com");
        Assert.AreEqual("Johnny", updatedUser.FirstName);
        Assert.AreEqual("Dough", updatedUser.LastName);
    }
}