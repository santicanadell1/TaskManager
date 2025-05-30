using DataAccess.Exceptions.NotificationRepositoryExceptions;
using Domain;

namespace DataAccess.Test;

[TestClass]
public class NotificationRepositoryTest
{
    private AppDbContext _context;
    private InMemoryAppContextFactory _contextFactory;
    private NotificationRepository _notificationRepository;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryAppContextFactory();
        _context = _contextFactory.CreateDbContext();
        _notificationRepository = new NotificationRepository(_context);
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context.Database.EnsureDeleted();
    }

    [TestMethod]
    public void NewNotificationRepository_WhenRepositoryIsCreated_ShouldNotBeNull()
    {
        Assert.IsNotNull(_notificationRepository);
    }

    [TestMethod]
    public void AddNewNotification_WhenAddNewNotification_ListShouldContainNotification()
    {
        var notification = new Notification(false, "New notification", new Project());
        _notificationRepository.Add(notification);
        _context.SaveChanges();
        Assert.IsTrue(_notificationRepository.GetAll().Count == 1);
        Assert.IsTrue(_notificationRepository.GetAll().Contains(notification));
    }

    [TestMethod]
    public void AddNewNotification_WhenGettingANotification_ShouldReturnNotification()
    {
        var notification1 = new Notification(false, "New notification", new Project());
        var notification2 = new Notification(true, "Another notification", new Project());
        _notificationRepository.Add(notification1);
        _notificationRepository.Add(notification2);
        _context.SaveChanges();
        
        var notification = _notificationRepository.Get(n => n.Description == "New notification");

        Assert.AreEqual(notification1, notification);
    }

    [TestMethod]
    public void UpdateNotification_WhenGettingTheNotification_ShouldBeDifferentFromOriginalNotification()
    {
        var notification1 = new Notification(false, "Old notification", new Project());
        var notification2 = new Notification(true, "Updated notification", new Project());
        _notificationRepository.Add(notification1);
        _context.SaveChanges();

        _notificationRepository.Update(notification1, notification2);
        _context.SaveChanges();

        Assert.AreNotEqual(notification1, _notificationRepository.Get(n => n.Description == "Updated notification"));
    }

    [TestMethod]
    [ExpectedException(typeof(NotificationNotFoundException))]
    public void UpdateNotification_WhenNotificationIsNotFound_ShouldThrowNotificationNotFoundException()
    {
        var notification1 = new Notification(false, "Old notification", new Project());
        var notification2 = new Notification(true, "Updated notification", new Project());
        _notificationRepository.Add(notification1);
        _context.SaveChanges();

        var notification3 = new Notification(false, "Nonexistent notification", new Project());
        _notificationRepository.Update(notification3, notification2);
        _context.SaveChanges();

    }

    [TestMethod]
    public void DeleteNotification_WhenGettingTheNotification_ShouldBeNull()
    {
        var notification1 = new Notification(false, "Notification to be deleted", new Project());
        var notification2 = new Notification(true, "Another notification", new Project());
        _notificationRepository.Add(notification1);
        _notificationRepository.Add(notification2);
        _context.SaveChanges();

        _notificationRepository.Delete(notification1);
        _context.SaveChanges();

        Assert.IsNull(_notificationRepository.Get(n => n.Description == "Notification to be deleted"));
    }
}