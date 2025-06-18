using Domain.Exceptions.NotificationExceptions;

namespace Domain.Test;

[TestClass]
public class NotificationTest
{
    private Project project;


    [TestInitialize]
    public void Initialize()
    {
        project = new Project("Project", "Description", DateTime.Today);
    }

    [TestMethod]
    [ExpectedException(typeof(NotificationDescriptionException))]
    public void NewNotification_WhenDescriptionIsEmpty_ShouldThrowNotificationDescriptionException()
    {
        Notification not;

        not = new Notification(false, "", project);
    }

    [TestMethod]
    public void MarkRead_ShouldSetReadToTrue()
    {
        Notification not;
        not = new Notification(false, "Some description", project);


        not.MarkRead();


        Assert.IsTrue(not.IsRead);
    }

    [TestMethod]
    [ExpectedException(typeof(NotificationDescriptionException))]
    public void NewNotification_WhenDescriptionIsWhiteSpace_ShouldThrowNotificationDescriptionException()
    {
        var not = new Notification(false, "    ", project);
    }

    [TestMethod]
    public void NewNotification_WhenCreated_ShouldHaveReadAsFalse()
    {
        var not = new Notification(false, "Some description", project);


        Assert.IsFalse(not.IsRead);
    }

    [TestMethod]
    public void Description_WhenSetToValidValue_ThenNoExceptionThrown()
    {
        var not = new Notification(false, "Valid description", project);


        Assert.AreEqual("Valid description", not.Description);
    }

    [TestMethod]
    public void Description_WhenChanged_ThenUpdatedSuccessfully()
    {
        var not = new Notification(false, "Initial description", project);


        not.Description = "Updated description";


        Assert.AreEqual("Updated description", not.Description);
    }

    [TestMethod]
    [ExpectedException(typeof(NotificationException))]
    public void Project_WhenProjectIsNull_ThrowException()
    {
        var not = new Notification(false, "Initial description", null);
    }
}