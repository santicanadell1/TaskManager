using DataAccess;
using Domain;
using Service.Models;

namespace Service.Test;

[TestClass]
public class NotificationServiceTest
{
    [TestMethod]
    public void NotificationService_WhenGettingAllNotificationsForUser_ThenReturnNotifications()
    {
        Notification notification1 = new Notification(false, "Description 1");
        Notification notification2 = new Notification(false, "Description 2");
        List<Notification> notifications = new List<Notification> { notification1, notification2 };
        User user1 = new User("Name 1", "LastName 1", "Email1@example.com", DateTime.Today, "Password1");
        User user2 = new User("Name2", "LastName2", "Email2@example.com", DateTime.Today, "Password2");
        List<User> members = new List<User> { user1, user2 };
        Project project = new Project();
        project.Name = "Project 1";
        project.Description = "Description 1";
        project.StartDate = DateTime.Today;
        project.Notifications = notifications;
        project.Members = members;
        Project project2 = new Project();
        project2.Name = "Project 2";
        project2.Description = "Description 2";
        project2.StartDate = DateTime.Today;
        project2.Notifications = notifications;
        project2.Members = new List<User> { user1 };
        InMemoryDatabase database = new InMemoryDatabase();
        database.projects.AddProject(project);
        database.projects.AddProject(project2);
        database.users.AddUser(user1);
        database.users.AddUser(user2);
        NotificationService notificationService = new NotificationService(database);

        List<NotificationDTO> notificationForUser1 = notificationService.GetNotificationsForUser(user1.Email);
        List<NotificationDTO> notificationForUser2 = notificationService.GetNotificationsForUser(user2.Email);

        Assert.IsTrue(notificationForUser1.Count == 4);
        Assert.IsTrue(notificationForUser2.Count == 2);
    }
}