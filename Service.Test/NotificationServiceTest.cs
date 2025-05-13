using DataAccess;
using Domain;
using Domain.Exceptions;
using Service;
using Service.Models;

[TestClass]
public class NotificationServiceTest
{
    private InMemoryDatabase dataAccess;
    private NotificationService _notificationService;
    private AdminPService _adminService;
    private Login _loginService;
    private UserService _userService;

    [TestInitialize]
    public void SetUp()
    {
        dataAccess = new InMemoryDatabase();
        _notificationService = new NotificationService(dataAccess);
        _adminService = new AdminPService(dataAccess);
        _loginService = new Login(dataAccess);
        _userService = new UserService(dataAccess);
        CreateAndAddUsers();
        CreateAndAddProjects();
    }

    private void CreateAndAddUsers()
    {
        UserDTO user1 = new UserDTO{FirstName = "Name 1", LastName = "LastName 1", Email = "Email1@example.com", Birthday = DateTime.Today, Password = "Password1@"};
        user1.Roles = new List<RolDTO> { RolDTO.AdminProject };
        UserDTO user2 = new UserDTO
        {
            FirstName = "Name 2", LastName = "LastName 2", Email = "Email2@example.com", Birthday = DateTime.Today,
            Password = "Password2@", Roles = new List<RolDTO> { }
        };
        _userService.AddUser(user1);
        _userService.AddUser(user2);
        _loginService.LoginUser("Email1@example.com", "Password1@");
    }

    private void CreateAndAddProjects()
    {
        User user1 = dataAccess.users.Get(u => u.Email == "Email1@example.com");
        User user2 = dataAccess.users.Get(u => u.Email == "Email2@example.com");

        Project project1 = new Project
        {
            Name = "Project 1",
            Description = "Description 1",
            StartDate = DateTime.Today,
            Members = new List<User> { user1, user2 },
        };

        Project project2 = new Project
        {
            Name = "Project 2",
            Description = "Description 2",
            StartDate = DateTime.Today,
            Members = new List<User> { user1 },
        };

        dataAccess.projects.AddProject(project1);
        dataAccess.projects.AddProject(project2);
    }

    [TestMethod]
    public void GetNotificationsForUser_WhenUserHasNotifications_ThenReturnNotifications()
    {
        var userEmail = "Email1@example.com";
        var notificationDTO = new NotificationDTO { Read = false, Description = "Test notification", Project = _adminService.GetProjectByName("Project 1")};
        _notificationService.CreateNotification(notificationDTO);

        var result = _notificationService.GetNotificationsForUser(userEmail);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Test notification", result[0].Description);
    }

    [TestMethod]
    public void AddNotificationToProject_WhenNotificationIsAdded_ThenNotificationShouldBeAddedToAllProjectMembers()
    {   
        var projectName = "Project 1";
        var notificationDTO = new NotificationDTO { Read = false, Description = "New Project Notification", Project = _adminService.GetProjectByName(projectName) }; 
        _notificationService.CreateNotification(notificationDTO);
        var user1 = dataAccess.users.Get(u => u.Email == "Email1@example.com");
        var user2 = dataAccess.users.Get(u => u.Email == "Email2@example.com");

        Assert.AreEqual(1, user1.Notifications.Count);
        Assert.AreEqual(1, user2.Notifications.Count);
    }

    
}