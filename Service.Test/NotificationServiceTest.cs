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
        User user1 = dataAccess.users.Get(u => u.Email == "Email1@example.com");
        User user2 = dataAccess.users.Get(u => u.Email == "Email2@example.com");

        Project project1 = new Project
        {
            Name = "Project 1",
            Description = "Description 1",
            StartDate = DateTime.Today,
            Members = new List<User> { user1, user2 },
            Notifications = new List<Notification>()
        };

        Project project2 = new Project
        {
            Name = "Project 2",
            Description = "Description 2",
            StartDate = DateTime.Today,
            Members = new List<User> { user1 },
            Notifications = new List<Notification>()
        };

        dataAccess.projects.AddProject(project1);
        dataAccess.projects.AddProject(project2);
    }

    [TestMethod]
    public void GetNotificationsForUser_WhenUserHasNotifications_ThenReturnNotifications()
    {
        var userEmail = "Email1@example.com";
        var notificationDTO = new NotificationDTO { Read = false, Description = "Test notification" };
        _notificationService.AddNotificationToProject("Project 1", notificationDTO);

        var result = _notificationService.GetNotificationsForUser(userEmail);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Test notification", result[0].Description);
    }

    [TestMethod]
    public void AddNotificationToProject_WhenNotificationIsAdded_ThenNotificationShouldBeAddedToAllProjectMembers()
    {
        var notificationDTO = new NotificationDTO { Read = false, Description = "New Project Notification" };
        var projectName = "Project 1";

        _notificationService.AddNotificationToProject(projectName, notificationDTO);

        var project = dataAccess.projects.GetProject(p => p.Name == projectName);
        Assert.AreEqual(1, project.Notifications.Count);

        var user1 = dataAccess.users.Get(u => u.Email == "Email1@example.com");
        var user2 = dataAccess.users.Get(u => u.Email == "Email2@example.com");

        Assert.AreEqual(1, user1.Notifications.Count);
        Assert.AreEqual(1, user2.Notifications.Count);
    }

    [TestMethod]
    public void RemoveNotificationFromProject_WhenNotificationExists_ThenNotificationShouldBeRemoved()
    {
        var notificationDTO = new NotificationDTO { Read = false, Description = "Notification to Remove" };
        _notificationService.AddNotificationToProject("Project 1", notificationDTO);

        var project = dataAccess.projects.GetProject(p => p.Name == "Project 1");
        var notificationToRemove = project.Notifications.First(n => n.Description == "Notification to Remove");

        _notificationService.RemoveNotificationFromProject("Project 1", notificationToRemove.Id);

        var updatedProject = dataAccess.projects.GetProject(p => p.Name == "Project 1");
        Assert.IsFalse(updatedProject.Notifications.Any(n => n.Description == "Notification to Remove"));
    }

    [TestMethod]
    public void MarkNotificationAsRead_WhenNotificationExists_ThenMarkAsReadAndRemoveFromUser()
    {
        var notificationDTO = new NotificationDTO { Read = false, Description = "Test notification to mark as read" };
        _notificationService.AddNotificationToProject("Project 1", notificationDTO);

        var userEmail = "Email1@example.com";
        var user = dataAccess.users.Get(u => u.Email == userEmail);
        var notificationId = user.Notifications.First(n => n.Description == "Test notification to mark as read").Id;

        _notificationService.MarkNotificationAsRead(notificationId, userEmail);

        var updatedUser = dataAccess.users.Get(u => u.Email == userEmail);
        Assert.AreEqual(0, updatedUser.Notifications.Count);
        Assert.IsTrue(updatedUser.Notifications.All(n => n.Read == true));
    }

    [TestMethod]
    [ExpectedException(typeof(NotificationNotFoundException))]
    public void MarkNotificationAsRead_WhenNotificationDoesNotExist_ThenThrowException()
    {
        var userEmail = "Email1@example.com";
        var invalidNotificationId = 999;

        _notificationService.MarkNotificationAsRead(invalidNotificationId, userEmail);
    }
}