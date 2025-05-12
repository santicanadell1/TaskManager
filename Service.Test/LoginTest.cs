using DataAccess;
using Domain;
using Service;
using Service.Exceptions;
using Service.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class LoginTests
{
    private Login _login;
    private InMemoryDatabase _inMemoryDatabase;
    private PasswordManager _passwordManager;

    [TestInitialize]
    public void Setup()
    {
        _inMemoryDatabase = new InMemoryDatabase();
        _login = new Login(_inMemoryDatabase);
        _passwordManager = new PasswordManager();
    }

    [TestMethod]
    public void Login_ShouldLoginSuccessfully_WithValidCredentials()
    {
        var email = "john.doe@example.com";
        var password = "Password123@";
        var roles = new List<Rol> { Rol.AdminSystem, Rol.AdminProject };

        var userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Roles = roles
        };

        var userService = new UserService(_inMemoryDatabase);
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
        var roles = new List<Rol> { Rol.AdminSystem, Rol.AdminProject };

        var userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Roles = roles
        };

        var userService = new UserService(_inMemoryDatabase);
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
        var roles = new List<Rol> { Rol.AdminSystem, Rol.AdminProject };

        var userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = "Password123@",
            Roles = roles
        };

        var userService = new UserService(_inMemoryDatabase);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);
    }

    [TestMethod]
    public void IsAdminSystem_ShouldReturnTrue_WhenUserIsAdminSystem()
    {
        var email = "john.doe@example.com";
        var password = "Password123@";
        var roles = new List<Rol> { Rol.AdminSystem, Rol.AdminProject };

        var userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Roles = roles
        };

        var userService = new UserService(_inMemoryDatabase);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);
        
        Assert.IsTrue(_login.IsAdminSystem());
    }[TestMethod]
    public void IsAdminProject_ShouldReturnTrue_WhenUserIsAdminProject()
    {
        var email = "john.doe@example.com";
        var password = "Password123@";
        var roles = new List<Rol> { Rol.AdminSystem, Rol.AdminProject };

        var userDTO = new UserDTO
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = password,
            Roles = roles
        };

        var userService = new UserService(_inMemoryDatabase);
        userService.AddUser(userDTO);

        _login.LoginUser(email, password);
        
        Assert.IsTrue(_login.IsAdminProject());
    }
    
}