using DataAccess;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service;
using Service.Models;

[TestClass]
public class NotificationServiceTest
{
    private AdminPService _adminPService;
    private AdminPService _adminService;
    private AppDbContext _context;
    private InMemoryAppContextFactory _contextFactory;
    private Login _loginService;
    private NotificationService _notificationService;
    private IRepositoryManager _repositoryManager;
    private UserService _userService;

    [TestInitialize]
    public void SetUp()
    {
        _contextFactory = new InMemoryAppContextFactory();
        _context = _contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repositoryManager = new RepositoryManager(_context);

        _notificationService = new NotificationService(_repositoryManager);

        _adminService = new AdminPService(_repositoryManager);

        _loginService = new Login(_repositoryManager);
        _userService = new UserService(_repositoryManager);
        _adminPService = new AdminPService(_repositoryManager);

        CreateAndAddProjectsAndUsers();
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context?.Database.EnsureDeleted();
    }

    private void CreateAndAddProjectsAndUsers()
    {
        var user1 = new UserDTO
        {
            FirstName = "Name 1",
            LastName = "LastName 1",
            Email = "Email1@example.com",
            Birthday = DateTime.Today.AddYears(-18),
            Password = "Password1@",
            Roles = new List<RolDTO> { RolDTO.ProjectMember, RolDTO.AdminProject }
        };

        var user2 = new UserDTO
        {
            FirstName = "Name 2",
            LastName = "LastName 2",
            Email = "Email2@example.com",
            Birthday = DateTime.Today.AddYears(-18),
            Password = "Password2@",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };

        _userService.AddUser(user1);
        _userService.AddUser(user2);

        _loginService.LoginUser(user1.Email, user1.Password);

        var project1 = new ProjectDTO
        {
            Name = "Project 1",
            Description = "Description 1",
            StartDate = DateTime.Today
        };

        var project2 = new ProjectDTO
        {
            Name = "Project 2",
            Description = "Description 2",
            StartDate = DateTime.Today
        };

        _adminPService.CreateProject(project1);
        _adminPService.CreateProject(project2);

        _adminPService.AssignMembersToProject(project1.Name, new List<UserDTO> { user1, user2 });
        _adminPService.AssignMembersToProject(project2.Name, new List<UserDTO> { user1 });
    }

    [TestMethod]
    public void GetNotificationsForUser_WhenUserHasNotifications_ThenReturnNotifications()
    {
        var userEmail = "Email1@example.com";
        var notificationDTO = new NotificationDTO
        {
            Read = false,
            Description = "Test notification",
            Project = _adminService.GetProjectByName("Project 1")
        };
        _notificationService.CreateNotification(notificationDTO);

        var result = _notificationService.GetNotificationsForUser(userEmail);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Test notification", result[0].Description);
    }

    [TestMethod]
    public void AddNotificationToProject_WhenNotificationIsAdded_ThenNotificationShouldBeAddedToAllProjectMembers()
    {
        var projectName = "Project 1";
        var project = _repositoryManager.ProjectRepository.Get(p => p.Name == projectName);

        var notificationDTO = new NotificationDTO
        {
            Read = false,
            Description = "New Project Notification",
            Project = _adminService.GetProjectByName(projectName)
        };

        _notificationService.CreateNotification(notificationDTO);

        var user1 = _repositoryManager.UserRepository.Get(u => u.Email == "Email1@example.com");
        var user2 = _repositoryManager.UserRepository.Get(u => u.Email == "Email2@example.com");

        Assert.AreEqual(2, project.Members.Count);
        Assert.AreEqual(1, user1.Notifications.Count);
        Assert.AreEqual(1, user2.Notifications.Count);
    }

    [TestMethod]
    public void RemoveNotificationFromUser_WhenNotificationIsRemoved_ThenNotificationShouldBeRemovedFromUser()
    {
        var projectName = "Project 1";
        var notificationDTO = new NotificationDTO
        {
            Read = false,
            Description = "New Project Notification",
            Project = _adminService.GetProjectByName(projectName)
        };
        var notificationDTO2 = new NotificationDTO
        {
            Read = false,
            Description = "New Project Notification 2",
            Project = _adminService.GetProjectByName(projectName)
        };
        _notificationService.CreateNotification(notificationDTO);
        _notificationService.CreateNotification(notificationDTO2);

        var user1 = _repositoryManager.UserRepository.Get(u => u.Email == "Email1@example.com");

        Assert.AreEqual(2, user1.Notifications.Count);

        _notificationService.RemoveNotificationFromUser(user1.Email, user1.Notifications[0].Id);

        user1 = _repositoryManager.UserRepository.Get(u => u.Email == "Email1@example.com");
        Assert.AreEqual(1, user1.Notifications.Count);
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void RemoveNotificationFromUser_WhenUserIsIncorrect_ThenThrowsException()
    {
        var projectName = "Project 1";
        var notificationDTO = new NotificationDTO
        {
            Read = false,
            Description = "New Project Notification",
            Project = _adminService.GetProjectByName(projectName)
        };
        var notificationDTO2 = new NotificationDTO
        {
            Read = false,
            Description = "New Project Notification 2",
            Project = _adminService.GetProjectByName(projectName)
        };

        _notificationService.CreateNotification(notificationDTO);
        _notificationService.CreateNotification(notificationDTO2);
        _notificationService.RemoveNotificationFromUser("WrongEmail@example.com", 1);
    }
}