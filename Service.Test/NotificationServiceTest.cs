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
    private AdminPService _adminPService;
    private AppDbContext _context;
    private UserRepository _userRepository;
    private ProjectRepository _projectRepository;
    private NotificationRepository _notificationRepository;
    private TaskRepository _taskRepository;

    [TestInitialize]
    public void SetUp()
    {
        var contextFactory = new InMemoryAppContextFactory();
        _context = contextFactory.CreateDbContext();

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _userRepository = new UserRepository(_context);
        _projectRepository = new ProjectRepository(_context);
        _notificationRepository = new NotificationRepository(_context);
        _taskRepository = new TaskRepository(_context);

        _notificationService = new NotificationService(_userRepository, _projectRepository, _notificationRepository);
        _adminService =
            new AdminPService(_userRepository, _projectRepository, _notificationRepository, _taskRepository);
        _loginService = new Login(_userRepository);
        _userService = new UserService(_userRepository);
        CreateAndAddProjectsAndUsers();
    }

    private void CreateAndAddProjectsAndUsers()
    {
        var user1 = new User
        {
            FirstName = "Name 1", 
            LastName = "LastName 1", 
            Email = "Email1@example.com",
            Birthday = DateTime.Today.AddYears(-18), 
            Password = "Password1@",
            Roles = new List<Rol> { Rol.ProjectMember, Rol.AdminProject }
        };
        
        var user2 = new User
        {
            FirstName = "Name 2", 
            LastName = "LastName 2", 
            Email = "Email2@example.com",
            Birthday = DateTime.Today.AddYears(-18),
            Password = "Password2@", 
            Roles = new List<Rol> { Rol.ProjectMember }
        };

        _userRepository.AddUser(user1);
        _userRepository.AddUser(user2);

        var savedUser1Entity = _userRepository.Get(u => u.Email == "Email1@example.com");
        var savedUser2Entity = _userRepository.Get(u => u.Email == "Email2@example.com");

        var project1 = new Project()
        {
            Name = "Project 1",
            Description = "Description 1",
            StartDate = DateTime.Today,
            Members = new List<User> { savedUser1Entity, savedUser2Entity }
        };

        var project2 = new Project()
        {
            Name = "Project 2",
            Description = "Description 2",
            StartDate = DateTime.Today,
            Members = new List<User> { savedUser1Entity }
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
        var project = _projectRepository.GetProject(p => p.Name == "Project 1");

        var notificationDTO = new NotificationDTO
        {
            Read = false, 
            Description = "New Project Notification",
            Project = _adminService.GetProjectByName(projectName)
        };

        _notificationService.CreateNotification(notificationDTO);

        var user1 = _userRepository.Get(u => u.Email == "Email1@example.com");
        var user2 = _userRepository.Get(u => u.Email == "Email2@example.com");

        project = _projectRepository.GetProject(p => p.Name == "Project 1");

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
        
        var user1 = _userRepository.Get(u => u.Email == "Email1@example.com");
        Assert.AreEqual(2, user1.Notifications.Count);
        
        _notificationService.RemoveNotificationFromUser(user1.Email, user1.Notifications[0]);
        
        user1 = _userRepository.Get(u => u.Email == "Email1@example.com");
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