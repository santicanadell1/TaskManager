using DataAccess;
using Domain;
using Service;
using Service.Models;

[TestClass]
public class NotificationServiceTest
{
    private InMemoryDatabase dataAccess;
    private NotificationService _notificationService;

    [TestInitialize]
    public void SetUp()
    {
        dataAccess = new InMemoryDatabase();
        _notificationService = new NotificationService(dataAccess);
        CreateAndAddUsers();
        CreateAndAddProjects();
    }

    private void CreateAndAddUsers()
    {
        User user1 = new User("Name 1", "LastName 1", "Email1@example.com", DateTime.Today, "Password1");
        User user2 = new User("Name2", "LastName2", "Email2@example.com", DateTime.Today, "Password2");
        dataAccess.users.AddUser(user1);
        dataAccess.users.AddUser(user2);
    }

    private void CreateAndAddProjects()
    {
        Notification notification1 = new Notification(false, "Description 1");
        Notification notification2 = new Notification(false, "Description 2");
        List<Notification> notifications = new List<Notification> { notification1, notification2 };

        User user1 = dataAccess.users.GetAll().First(u => u.Email == "Email1@example.com");
        User user2 = dataAccess.users.GetAll().First(u => u.Email == "Email2@example.com");

        Project project1 = new Project
        {
            Name = "Project 1",
            Description = "Description 1",
            StartDate = DateTime.Today,
            Notifications = notifications,
            Members = new List<User> { user1, user2 }
        };

        Project project2 = new Project
        {
            Name = "Project 2",
            Description = "Description 2",
            StartDate = DateTime.Today,
            Notifications = notifications,
            Members = new List<User> { user1 }
        };
        dataAccess.notifications.AddNotification(notification1);
        dataAccess.notifications.AddNotification(notification2);
        dataAccess.projects.AddProject(project1);
        dataAccess.projects.AddProject(project2);
    }

    [TestMethod]
    public void NotificationService_WhenGettingAllNotificationsForUser_ThenReturnNotifications()
    {
        List<NotificationDTO> notificationForUser1 = _notificationService.GetNotificationsForUser("Email1@example.com");
        List<NotificationDTO> notificationForUser2 = _notificationService.GetNotificationsForUser("Email2@example.com");

        Assert.IsTrue(notificationForUser1.Count == 4);
        Assert.IsTrue(notificationForUser2.Count == 2);
    }

    [TestMethod]
    public void AddNotificationToProject_WhenNotificationIsAdded_ThenNotificationShouldBeAssociatedWithProject()
    {
        var projectId = "Project 1";
        var notificationDTO = new NotificationDTO
        {
            Read = false,
            Description = "New Notification for Project 1",
        };

        _notificationService.AddNotificationToProject(projectId, notificationDTO);

        var project = dataAccess.projects.GetProject(p => p.Name == projectId);
        var addedNotification =
            project.Notifications.FirstOrDefault(n => n.Description == "New Notification for Project 1");

        Assert.IsNotNull(addedNotification);
        Assert.AreEqual("New Notification for Project 1", addedNotification.Description);
        Assert.IsFalse(addedNotification.Read);
    }

    [TestMethod]
    public void RemoveNotificationFromProject_WhenNotificationExists_ThenNotificationShouldBeRemoved()
    {
        var projectId = "Project 1";
        int notificationId = 1;
        _notificationService.RemoveNotificationFromProject(projectId, notificationId);

        var project = dataAccess.projects.GetProject(p => p.Name == projectId);
        var removedNotification = project.Notifications.FirstOrDefault(n => n.Id == notificationId);

        Assert.IsNull(removedNotification);
    }

    [TestMethod]
    public void MarkNotificationAsRead_WhenNotificationExists_ThenRemoveNotificationFromProjectAndRepository()
    {
        var projectId = "Project 1";
        var notificationId = 1;

        _notificationService.MarkNotificationAsRead(projectId, notificationId);

        var project = dataAccess.projects.GetProject(p => p.Name == projectId);
        var removedNotification = project.Notifications.FirstOrDefault(n => n.Id == notificationId);
        Assert.IsNull(removedNotification);

        var notificationInRepository = dataAccess.notifications.Get(n => n.Id == notificationId);
        Assert.IsNull(notificationInRepository);
    }
}