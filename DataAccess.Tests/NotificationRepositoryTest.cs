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
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        _notificationRepository = new NotificationRepository(_context);
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context?.Database.EnsureDeleted();
    }

    [TestMethod]
    public void NewNotificationRepository_WhenRepositoryIsCreated_ShouldNotBeNull()
    {
        Assert.IsNotNull(_notificationRepository);
    }

    [TestMethod]
    public void AddNewNotification_WhenAddNewNotification_ListShouldContainNotification()
    {
        Project project = new Project { Name = "Project 1", Description = "Project 1 description" };
        Notification notification = new Notification(false, "New notification", project);
        _notificationRepository.Add(notification);

        Assert.IsTrue(_notificationRepository.GetAll().Count == 1);
        Assert.IsTrue(_notificationRepository.GetAll().Contains(notification));
    }

    [TestMethod]
    public void AddNewNotification_WhenGettingANotification_ShouldReturnNotification()
    {
        Project project = new Project { Name = "Project 1", Description = "Project 1 description" };
        Notification notification1 = new Notification(false, "New notification", project);
        Notification notification2 = new Notification(true, "Another notification", project);
        _notificationRepository.Add(notification1);
        _notificationRepository.Add(notification2);

        Notification notification = _notificationRepository.Get(n => n.Description == "New notification");

        Assert.AreEqual(notification1, notification);
    }

    [TestMethod]
    public void UpdateNotification_WhenGettingTheNotification_ShouldBeDifferentFromOriginalNotification()
    {
        Project project = new Project { Name = "Project 1", Description = "Project 1 description" };
        Notification notification1 = new Notification(false, "Old notification", project);
        Notification notification2 = new Notification(true, "Updated notification", project);
        _notificationRepository.Add(notification1);
        notification2.Id = _notificationRepository.Get(n => n.Description == "Old notification").Id;

        _notificationRepository.Update(notification2);

        Notification updatedNotification = _notificationRepository.Get(n => n.Description == "Updated notification");

        Assert.IsNotNull(updatedNotification);
        Assert.AreEqual("Updated notification", updatedNotification.Description);
        Assert.AreEqual(true, updatedNotification.Read);
    }


    [TestMethod]
    [ExpectedException(typeof(NotificationNotFoundException))]
    public void UpdateNotification_WhenNotificationIsNotFound_ShouldThrowNotificationNotFoundException()
    {
        Project project = new Project { Name = "Project 1", Description = "Project 1 description" };
        Notification notification1 = new Notification(false, "Old notification", project);
        Notification notification2 = new Notification(true, "Updated notification", project);
        _notificationRepository.Add(notification1);

        _notificationRepository.Update(notification2);
    }

    [TestMethod]
    public void DeleteNotification_WhenGettingTheNotification_ShouldBeNull()
    {
        Project project = new Project { Name = "Project 1", Description = "Project 1 description" };
        Notification notification1 = new Notification(false, "Notification to be deleted", project);
        Notification notification2 = new Notification(true, "Another notification", project);
        _notificationRepository.Add(notification1);
        _notificationRepository.Add(notification2);

        _notificationRepository.Delete(notification1);

        Assert.IsNull(_notificationRepository.Get(n => n.Description == "Notification to be deleted"));
    }
}