using DataAccess;
using Domain;
using Service.Models;

namespace Service.Test;

[TestClass]
public class NotificationServiceTest
{
    

    [TestMethod]
    public void NotificationService_WhenTransformingNotificationIntoDTO_ThenReturnNotificationDTO()
    {
        Notification notification1 = new Notification(false, "Description 1");
        NotificationDTO notificationDto = NotificationService.FromEntity(notification1);
        NotificationDTO notificationDto2 = new NotificationDTO();
        notificationDto2.Read = false;
        notificationDto2.Description = "Description 1";
        Assert.IsTrue(notificationDto.Read == notificationDto2.Read && notificationDto.Description == notificationDto2.Description );
    }

    [TestMethod]
    public void NotificationService_WhenGettingAllNotificationsForUser_ThenReturnNotifications()
    {
        Notification notification1 = new Notification(false, "Description 1");
        Notification notification2 = new Notification(false, "Description 2");
        List<Notification> notifications = new List<Notification> { notification1, notification2 };
        User user1 = new User("Name 1" , "LastName 1" , "Email1@example.com", DateTime.Today, "Password1");
        User user2 = new User("Name2" , "LastName2" , "Email2@example.com", DateTime.Today, "Password2");
        List<User> members = new List<User> { user1, user2 };
        Project project = new Project();
        project.Name = "Project 1";
        project.Description = "Description 1";
        project.StartDate = DateTime.Today;
        project.Notifications = notifications;
        project.Members = members;
        InMemoryDatabase database = new InMemoryDatabase();
        database.projects.AddProject(project);
        database.users.AddUser(user1);
        database.users.AddUser(user2);
        NotificationService notificationService = new NotificationService(database);

        List<NotificationDTO> notificationForUser1 = notificationService.getNotificationsFor(user1);
        
        Assert.IsTrue(notificationForUser1.Count == 2);
    }
}