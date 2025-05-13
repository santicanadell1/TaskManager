using Domain;
using Domain.Exceptions.NotificationRepositoryExceptions;
namespace DataAccess.Test;


[TestClass]
public class NotificationRepositoryTest
{

    [TestMethod]
    public void NewNotificationRepository_WhenRepositoryIsCreated_ShouldNotBeNull()
    {
        NotificationRepository notificationRepository;

        notificationRepository = new NotificationRepository();

        Assert.IsNotNull(notificationRepository);
    }
    
    [TestMethod]
    public void AddNewNotification_WhenAddNewNotification_ListShouldContainNotification()
    {
        NotificationRepository notificationRepository = new NotificationRepository();
        Notification notification = new Notification(false, "New notification", new Project());

        notificationRepository.AddNotification(notification);

        Assert.IsTrue(notificationRepository.GetAll().Contains(notification));
    }
    [TestMethod]
    public void AddNewNotification_WhenGettingANotification_ShouldReturnNotification()
    {
        NotificationRepository notificationRepository = new NotificationRepository();
        Notification notification1 = new Notification(false, "New notification", new Project());
        Notification notification2 = new Notification(true, "Another notification", new Project());
        notificationRepository.AddNotification(notification1);
        notificationRepository.AddNotification(notification2);

        Notification notification3 = notificationRepository.Get(n => n.Description == "New notification");

        Assert.AreEqual(notification1, notification3);
    }
    
    [TestMethod]
    public void UpdateNotification_WhenGettingTheNotification_ShouldBeDifferentFromOriginalNotification()
    {
        NotificationRepository notificationRepository = new NotificationRepository();
        Notification notification1 = new Notification(false, "Old notification" , new Project());
        Notification notification2 = new Notification(true, "Updated notification" , new Project());
        notificationRepository.AddNotification(notification1);

        notificationRepository.Update(notification1, notification2);

        Assert.AreNotEqual(notification1, notificationRepository.Get(n => n.Description == "Updated notification"));
    }
    [TestMethod]
    [ExpectedException(typeof(NotificationNotFoundException))]
    public void UpdateNotification_WhenNotificationIsNotFound_ShouldThrowNotificationNotFoundException()
    {
        NotificationRepository notificationRepository = new NotificationRepository();
        Notification notification1 = new Notification(false, "Old notification" , new Project());
        Notification notification2 = new Notification(true, "Updated notification" , new Project());
        notificationRepository.AddNotification(notification1);

        Notification notification3 = new Notification(false, "Nonexistent notification" , new Project());
        notificationRepository.Update(notification3, notification2);
    }
    
    [TestMethod]
    public void DeleteNotification_WhenGettingTheNotification_ShouldBeNull()
    {
        NotificationRepository notificationRepository = new NotificationRepository();
        Notification notification1 = new Notification(false, "Notification to be deleted", new Project());
        Notification notification2 = new Notification(true, "Another notification" , new Project());
        notificationRepository.AddNotification(notification1);
        notificationRepository.AddNotification(notification2);

        notificationRepository.Delete(notification1);

        Assert.IsNull(notificationRepository.Get(n => n.Description == "Notification to be deleted"));
    }


    
    
    
}