using DataAccess;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Service.Exceptions.UserServiceExceptions;
using Service.Models;

namespace Service.Test;

[TestClass]
public class UserServiceTest
{
    private AppDbContext _context;
    private UserService _userService;
    private InMemoryAppContextFactory _contextFactory;

    private UserRepository _userRepository;

    [TestInitialize]
    public void TestSetUp()
    {
        _contextFactory = new InMemoryAppContextFactory();
        _context = _contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _userRepository = new UserRepository(_context);

        _userService = new UserService(_userRepository);
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
        List<RolDTO> rols = new List<RolDTO>();
        rols.Add(RolDTO.ProjectMember);

        UserService _userService = new UserService(_userRepository);

        UserDTO userDTO1 = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = rols
        };

        _userService.AddUser(userDTO1);

        UserDTO userDTO2 = new UserDTO
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
        List<RolDTO> rols = new List<RolDTO>();
        rols.Add(RolDTO.ProjectMember);

        UserDTO userToUpdate = new UserDTO
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
        List<RolDTO> rols = new List<RolDTO> { RolDTO.ProjectMember };
        UserDTO userDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = rols
        };

        _userService.AddUser(userDTO);


        List<UserDTO> users = _userService.GetUsers();
        Assert.AreEqual(1, users.Count);
        Assert.AreEqual("john.doe@example.com", users[0].Email);
    }

    [TestMethod]
    public void UpdateUser_ShouldUpdateUser_WhenUserExists()
    {
        List<RolDTO> rols = new List<RolDTO> { RolDTO.ProjectMember };
        UserDTO userDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = rols
        };

        _userService.AddUser(userDTO);

        int? id = _userRepository.Get(user => user.Email == userDTO.Email).Id;


        UserDTO updatedUserDTO = new UserDTO
        {
            FirstName = "Johnny",
            LastName = "Dough",
            Email = "john.doe@example.com",
            Password = "NewPassword123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = rols,
            Id = id
        };


        _userService.UpdateUser(updatedUserDTO);


        UserDTO updatedUser = _userService.GetUser("john.doe@example.com");
        Assert.AreEqual("Johnny", updatedUser.FirstName);
        Assert.AreEqual("Dough", updatedUser.LastName);
    }
}