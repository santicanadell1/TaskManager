using Domain;
namespace DataAccess.Test;


[TestClass]
public class NotificationRepositoryTest
{
    
    [TestMethod]
    public void NewNotificationRepository_WhenRepositoryIsCreated_ShouldNotBeNull()
    {
        // Arrange
        NotificationRepository notificationRepository;

        // Act
        notificationRepository = new NotificationRepository();

        // Assert
        Assert.IsNotNull(notificationRepository);
    }
    
    [TestMethod]
    public void AddNewNotification_WhenAddNewNotification_ListShouldContainNotification()
    {
        // Arrange
        NotificationRepository notificationRepository = new NotificationRepository();
        Notification notification = new Notification(false, "New notification");

        // Act
        notificationRepository.AddNotification(notification);

        // Assert
        Assert.IsTrue(notificationRepository.GetAll().Contains(notification));
    }
    
    [TestMethod]
    public void AddNewNotification_WhenGettingANotification_ShouldReturnNotification()
    {
        // Arrange
        NotificationRepository notificationRepository = new NotificationRepository();
        Notification notification1 = new Notification(false, "New notification");
        Notification notification2 = new Notification(true, "Another notification");
        notificationRepository.AddNotification(notification1);
        notificationRepository.AddNotification(notification2);

        // Act
        Notification notification3 = notificationRepository.Get(n => n.Description == "New notification");

        // Assert
        Assert.AreEqual(notification1, notification3);
    }
    
    [TestMethod]
    public void UpdateNotification_WhenGettingTheNotification_ShouldBeDifferentFromOriginalNotification()
    {
        // Arrange
        NotificationRepository notificationRepository = new NotificationRepository();
        Notification notification1 = new Notification(false, "Old notification");
        Notification notification2 = new Notification(true, "Updated notification");
        notificationRepository.AddNotification(notification1);

        // Act
        notificationRepository.Update(notification1, notification2);

        // Assert
        Assert.AreNotEqual(notification1, notificationRepository.Get(n => n.Description == "Updated notification"));
    }




}