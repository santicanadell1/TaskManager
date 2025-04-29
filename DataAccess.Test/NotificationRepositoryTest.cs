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


}