using DataAccess;
using Domain;
using Service.Models;

namespace Service.Test;

[TestClass]
public class NotificationServiceTest
{
    

    [TestMethod]
    public void NotificationService_WhenTransformingNotificationIntoDTO_ThenReturnNotificationDTO()
    {
        Notification notification1 = new Notification(false, "Description 1");
        NotificationDTO notificationDto = NotificationService.FromEntity(notification1);
        NotificationDTO notificationDto2 = new NotificationDTO();
        notificationDto2.Read = false;
        notificationDto2.Description = "Description 1";
        Assert.AreEqual(notificationDto,notificationDto2 );
    }
}