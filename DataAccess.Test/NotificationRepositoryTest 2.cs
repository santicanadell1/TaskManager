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