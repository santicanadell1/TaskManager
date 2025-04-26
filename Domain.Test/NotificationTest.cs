namespace Domain.Test;


[TestClass]


public class NotificationTest
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewNotification_WhenDescriptionIsEmpty_ShouldThrowException()
    {

        //Arrange
        Notification not;
        //Act
        not = new Notification(false, "");
    }
}
 