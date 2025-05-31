using DataAccess;
using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;
using Service;
using Service.Models;

[TestClass]
public class NotificationServiceTest
{
    private AdminPService _adminService;
    private Login _loginService;
    private NotificationService _notificationService;
    private UserService _userService;
    private AppDbContext dataAccess;
    private UserRepository _userRepository;
    private ProjectRepository _projectRepository;
    private NotificationRepository _notificationRepository;
    private TaskRepository _taskRepository;
    private ResourceRepository _resourceRepository;

    [TestInitialize]
    public void SetUp()
    {
        _userRepository = new UserRepository(dataAccess);
        _notificationService = new NotificationService(_userRepository, _projectRepository, _notificationRepository, _taskRepository, _resourceRepository);
        _adminService = new AdminPService(_userRepository, _projectRepository,_notificationRepository, _taskRepository, _resourceRepository);
        _loginService = new Login(_userRepository);
        _userService = new UserService(_userRepository);
        CreateAndAddUsers();
        CreateAndAddProjects();
    }

    private void CreateAndAddUsers()
    {
        var user1 = new UserDTO
        {
            FirstName = "Name 1", LastName = "LastName 1", Email = "Email1@example.com",
            Birthday = DateTime.Today.AddYears(-18), Password = "Password1@"
        };
        user1.Roles = new List<RolDTO> { RolDTO.AdminProject };
        var user2 = new UserDTO
        {
            FirstName = "Name 2", LastName = "LastName 2", Email = "Email2@example.com",
            Birthday = DateTime.Today.AddYears(-18),
            Password = "Password2@", Roles = new List<RolDTO>()
        };
        _userService.AddUser(user1);
        _userService.AddUser(user2);
        _loginService.LoginUser("Email1@example.com", "Password1@");
    }

    private void CreateAndAddProjects()
    {
        var user1 = _userRepository.Get(u => u.Email == "Email1@example.com");
        var user2 = _userRepository.Get(u => u.Email == "Email2@example.com");

        var project1 = new Project
        {
            Name = "Project 1",
            Description = "Description 1",
            StartDate = DateTime.Today,
            Members = new List<User> { user1, user2 }
        };

        var project2 = new Project
        {
            Name = "Project 2",
            Description = "Description 2",
            StartDate = DateTime.Today,
            Members = new List<User> { user1 }
        };

        _projectRepository.AddProject(project1);
        _projectRepository.AddProject(project2);
    }

    [TestMethod]
    public void GetNotificationsForUser_WhenUserHasNotifications_ThenReturnNotifications()
    {
        var userEmail = "Email1@example.com";
        var notificationDTO = new NotificationDTO
            { Read = false, Description = "Test notification", Project = _adminService.GetProjectByName("Project 1") };
        _notificationService.CreateNotification(notificationDTO);

        var result = _notificationService.GetNotificationsForUser(userEmail);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Test notification", result[0].Description);
    }

    [TestMethod]
    public void AddNotificationToProject_WhenNotificationIsAdded_ThenNotificationShouldBeAddedToAllProjectMembers()
    {
        var projectName = "Project 1";
        var notificationDTO = new NotificationDTO
        {
            Read = false, Description = "New Project Notification",
            Project = _adminService.GetProjectByName(projectName)
        };
        _notificationService.CreateNotification(notificationDTO);
        var user1 = _userRepository.Get(u => u.Email == "Email1@example.com");
        var user2 = _userRepository.Get(u => u.Email == "Email2@example.com");

        Assert.AreEqual(1, user1.Notifications.Count);
        Assert.AreEqual(1, user2.Notifications.Count);
    }

    [TestMethod]
    public void RemoveNotificationFromUser_WhenNotificationIsRemoved_ThenNotificationShouldBeRemovedFromUser()
    {
        var projectName = "Project 1";
        var notificationDTO = new NotificationDTO
        {
            Read = false, Description = "New Project Notification",
            Project = _adminService.GetProjectByName(projectName)
        };
        var notificationDTO2 = new NotificationDTO
        {
            Read = false, Description = "New Project Notification 2",
            Project = _adminService.GetProjectByName(projectName)
        };
        _notificationService.CreateNotification(notificationDTO);
        _notificationService.CreateNotification(notificationDTO2);
        var user1 = _userRepository.Get(u => u.Email == "Email1@example.com");
        Assert.AreEqual(2, user1.Notifications.Count);
        _notificationService.RemoveNotificationFromUser(user1.Email, user1.Notifications[0]);
        Assert.AreEqual(1, user1.Notifications.Count);
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void RemoveNotificationFromUser_WhenUserIsIncorrect_ThenThrowsException()
    {
        var projectName = "Project 1";
        var notificationDTO = new NotificationDTO
        {
            Read = false, Description = "New Project Notification",
            Project = _adminService.GetProjectByName(projectName)
        };
        var notificationDTO2 = new NotificationDTO
        {
            Read = false, Description = "New Project Notification 2",
            Project = _adminService.GetProjectByName(projectName)
        };
        _notificationService.CreateNotification(notificationDTO);
        _notificationService.CreateNotification(notificationDTO2);
        _notificationService.RemoveNotificationFromUser("WrongEmail@example.com", 1);
    }
}