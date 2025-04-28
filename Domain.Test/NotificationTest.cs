using Domain.Exceptions;

namespace Domain.Test
{
    [TestClass]
    public class NotificationTest
    {
        [TestMethod]
        [ExpectedException(typeof(NotificationDescriptionException))]
        public void NewNotification_WhenDescriptionIsEmpty_ShouldThrowNotificationDescriptionException()
        {
            // Arrange
            Notification not;
            // Act
            not = new Notification(false, "");
        }

        [TestMethod]
        public void MarkRead_ShouldSetReadToTrue()
        {
            // Arrange
            Notification not;
            not = new Notification(false, "Some description");

            // Act
            not.MarkRead();

            // Assert
            Assert.IsTrue(not.Read);
        }
    }
}