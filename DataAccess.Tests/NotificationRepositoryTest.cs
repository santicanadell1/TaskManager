using DataAccess.Exceptions.NotificationRepositoryExceptions;
using Domain;

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
        var notificationRepository = new NotificationRepository();
        var notification = new Notification(false, "New notification", new Project());

        notificationRepository.Add(notification);

        Assert.IsTrue(notificationRepository.GetAll().Contains(notification));
    }

    [TestMethod]
    public void AddNewNotification_WhenGettingANotification_ShouldReturnNotification()
    {
        var notificationRepository = new NotificationRepository();
        var notification1 = new Notification(false, "New notification", new Project());
        var notification2 = new Notification(true, "Another notification", new Project());
        notificationRepository.Add(notification1);
        notificationRepository.Add(notification2);

        var notification3 = notificationRepository.Get(n => n.Description == "New notification");

        Assert.AreEqual(notification1, notification3);
    }

    [TestMethod]
    public void UpdateNotification_WhenGettingTheNotification_ShouldBeDifferentFromOriginalNotification()
    {
        var notificationRepository = new NotificationRepository();
        var notification1 = new Notification(false, "Old notification", new Project());
        var notification2 = new Notification(true, "Updated notification", new Project());
        notificationRepository.Add(notification1);

        notificationRepository.Update(notification1, notification2);

        Assert.AreNotEqual(notification1, notificationRepository.Get(n => n.Description == "Updated notification"));
    }

    [TestMethod]
    [ExpectedException(typeof(NotificationNotFoundException))]
    public void UpdateNotification_WhenNotificationIsNotFound_ShouldThrowNotificationNotFoundException()
    {
        var notificationRepository = new NotificationRepository();
        var notification1 = new Notification(false, "Old notification", new Project());
        var notification2 = new Notification(true, "Updated notification", new Project());
        notificationRepository.Add(notification1);

        var notification3 = new Notification(false, "Nonexistent notification", new Project());
        notificationRepository.Update(notification3, notification2);
    }

    [TestMethod]
    public void DeleteNotification_WhenGettingTheNotification_ShouldBeNull()
    {
        var notificationRepository = new NotificationRepository();
        var notification1 = new Notification(false, "Notification to be deleted", new Project());
        var notification2 = new Notification(true, "Another notification", new Project());
        notificationRepository.Add(notification1);
        notificationRepository.Add(notification2);

        notificationRepository.Delete(notification1);

        Assert.IsNull(notificationRepository.Get(n => n.Description == "Notification to be deleted"));
    }
}