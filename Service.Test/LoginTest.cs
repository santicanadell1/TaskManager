using DataAccess;
using Service;
using Service.Exceptions.LoginExceptions;
using Service.Models;

[TestClass]
public class LoginTests
{
    private AppDbContext _context;
    private Login _login;
    private PasswordManager _passwordManager;
    private UserRepository _userRepository;

    [TestInitialize]
    public void Setup()
    {
        var contextFactory = new InMemoryAppContextFactory();
        _context = contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        
        _userRepository = new UserRepository(_context);
        
        _login = new Login(_userRepository);
        _passwordManager = new PasswordManager();
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context?.Database.EnsureDeleted();
    }

    [TestMethod]
    public void Login_ShouldLoginSuccessfully_WithValidCredentials()
    {
        var email = "john.doe@example.com";
        var password = "Password123@";
        var roles = new List<RolDTO> { RolDTO.AdminSystem, RolDTO.AdminProject };

        var userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Roles = roles,
            Birthday = DateTime.Parse("1990-01-01")
        };

        var userService = new UserService(_userRepository);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);

        var loggedUser = _login.GetLoggedUser();
        Assert.IsNotNull(loggedUser);
        Assert.AreEqual("John", loggedUser.FirstName);
        Assert.AreEqual("Doe", loggedUser.LastName);
        Assert.AreEqual("john.doe@example.com", loggedUser.Email);
    }

    [TestMethod]
    public void Logout_ShouldLogoutSuccessfully_WhenUserIsLoggedIn()
    {
        var email = "john.doe@example.com";
        var password = "Password123@";
        var roles = new List<RolDTO> { RolDTO.AdminSystem, RolDTO.AdminProject };

        var userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = roles
        };

        var userService = new UserService(_userRepository);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);

        var loggedUserBeforeLogout = _login.GetLoggedUser();
        Assert.IsNotNull(loggedUserBeforeLogout);

        _login.Logout();

        var loggedUserAfterLogout = _login.GetLoggedUser();
        Assert.IsNull(loggedUserAfterLogout);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidLoginCredentialsException))]
    public void Login_ShouldThrowInvalidLoginCredentialsException_WhenCredentialsAreIncorrect()
    {
        var email = "john.doe@example.com";
        var password = "WrongPassword@";
        var roles = new List<RolDTO> { RolDTO.AdminSystem, RolDTO.AdminProject };

        var userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = "Password123@",
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = roles
        };

        var userService = new UserService(_userRepository);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);
    }

    [TestMethod]
    public void IsAdminSystem_ShouldReturnTrue_WhenUserIsAdminSystem()
    {
        var email = "john.doe@example.com";
        var password = "Password123@";
        var roles = new List<RolDTO> { RolDTO.AdminSystem, RolDTO.AdminProject };

        var userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = roles
        };

        var userService = new UserService(_userRepository);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);

        Assert.IsTrue(_login.IsAdminSystem());
    }

    [TestMethod]
    public void IsAdminProject_ShouldReturnTrue_WhenUserIsAdminProject()
    {
        var email = "john.doe@example.com";
        var password = "Password123@";
        var roles = new List<RolDTO> { RolDTO.AdminSystem, RolDTO.AdminProject };

        var userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = roles
        };

        var userService = new UserService(_userRepository);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);

        Assert.IsTrue(_login.IsAdminProject());
    }

    [TestMethod]
    public void IsProjectMember_ShouldReturnTrue_WhenUserIsProjectMember()
    {
        var email = "john.doe@example.com";
        var password = "Password123@";
        var roles = new List<RolDTO> { RolDTO.ProjectMember };

        var userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = roles
        };

        var userService = new UserService(_userRepository);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);

        Assert.IsTrue(_login.IsProjectMember());
    }

    [TestMethod]
    public void UpdateLogedUser_ShouldUpdateLoggedUser_WhenUserIsLoggedIn()
    {
        var email = "john.doe@example.com";
        var password = "Password123@";
        var roles = new List<RolDTO> { RolDTO.ProjectMember };

        var userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Birthday = DateTime.Parse("1990-01-01"),
            Roles = roles
        };

        var userService = new UserService(_userRepository);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);

        var userUpdate = new UserDTO
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