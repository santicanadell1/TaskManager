using DataAccess;
using Domain;
using Service;
using Service.Exceptions.LoginExceptions;
using Service.Interface;
using Service.Models;

[TestClass]
public class LoginTests
{
    private AppDbContext _context;
    private Login _login;
    private InMemoryAppContextFactory _contextFactory;
    private IRepositoryManager _repositoryManager;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryAppContextFactory();
        _context = _contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        
        _repositoryManager = new RepositoryManager(_context);
        
        _login = new Login(_repositoryManager);
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context?.Database.EnsureDeleted();
    }

    [TestMethod]
    public void Login_ShouldLoginSuccessfully_WithValidCredentials()
    {
        string email = "john.doe@example.com";
        string password = "Password123@";
        List<RolDTO> roles = new List<RolDTO> { RolDTO.AdminSystem, RolDTO.AdminProject };

        UserDTO userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Roles = roles,
            Birthday = DateTime.Parse("1990-01-01")
        };

        UserService userService = new UserService(_repositoryManager);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);

        UserDTO loggedUser = _login.GetLoggedUser();
        Assert.IsNotNull(loggedUser);
        Assert.AreEqual("John", loggedUser.FirstName);
        Assert.AreEqual("Doe", loggedUser.LastName);
        Assert.AreEqual("john.doe@example.com", loggedUser.Email);
    }

    [TestMethod]
    public void Logout_ShouldLogoutSuccessfully_WhenUserIsLoggedIn()
    {
        string email = "john.doe@example.com";
        string password = "Password123@";
        List<RolDTO> roles = new List<RolDTO> { RolDTO.AdminSystem, RolDTO.AdminProject };

        UserDTO userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = roles
        };

        UserService userService = new UserService(_repositoryManager);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);

        UserDTO loggedUserBeforeLogout = _login.GetLoggedUser();
        Assert.IsNotNull(loggedUserBeforeLogout);

        _login.Logout();

        UserDTO loggedUserAfterLogout = _login.GetLoggedUser();
        Assert.IsNull(loggedUserAfterLogout);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidLoginCredentialsException))]
    public void Login_ShouldThrowInvalidLoginCredentialsException_WhenCredentialsAreIncorrect()
    {
        string email = "john.doe@example.com";
        string password = "WrongPassword@";
        List<RolDTO> roles = new List<RolDTO> { RolDTO.AdminSystem, RolDTO.AdminProject };

        UserDTO userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = roles
        };

        UserService userService = new UserService(_repositoryManager);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);
    }

    [TestMethod]
    public void IsAdminSystem_ShouldReturnTrue_WhenUserIsAdminSystem()
    {
        string email = "john.doe@example.com";
        string password = "Password123@";
        List<RolDTO> roles = new List<RolDTO> { RolDTO.AdminSystem, RolDTO.AdminProject };

        UserDTO userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = roles
        };

        UserService userService = new UserService(_repositoryManager);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);

        Assert.IsTrue(_login.IsAdminSystem());
    }

    [TestMethod]
    public void IsAdminProject_ShouldReturnTrue_WhenUserIsAdminProject()
    {
        string email = "john.doe@example.com";
        string password = "Password123@";
        List<RolDTO> roles = new List<RolDTO> { RolDTO.AdminSystem, RolDTO.AdminProject };

        UserDTO userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = roles
        };

        UserService userService = new UserService(_repositoryManager);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);

        Assert.IsTrue(_login.IsAdminProject());
    }

    [TestMethod]
    public void IsProjectMember_ShouldReturnTrue_WhenUserIsProjectMember()
    {
        string email = "john.doe@example.com";
        string password = "Password123@";
        List<RolDTO> roles = new List<RolDTO> { RolDTO.ProjectMember };

        UserDTO userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = roles
        };

        UserService userService = new UserService(_repositoryManager);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);

        Assert.IsTrue(_login.IsProjectMember());
    }

    [TestMethod]
    public void UpdateLogedUser_ShouldUpdateLoggedUser_WhenUserIsLoggedIn()
    {
        string email = "john.doe@example.com";
        string password = "Password123@";
        List<RolDTO> roles = new List<RolDTO> { RolDTO.ProjectMember };

        UserDTO userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = roles
        };

        UserService userService = new UserService(_repositoryManager);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);

        UserDTO userUpdate = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };
        _login.UpdateUser(userUpdate);
        Assert.IsFalse(_login.IsProjectMember());
        Assert.IsTrue(_login.IsAdminProject());
    }
}