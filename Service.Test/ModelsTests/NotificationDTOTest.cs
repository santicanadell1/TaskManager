using System.ComponentModel.DataAnnotations;
using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Service.Models;

[TestClass]

public class NotificationDTOTest
{
    [TestMethod]
    public void NewNotification_WhenReadIsTrue_ThenNotificationIsCreatedWithTrue()
    {
        
        var notification = new NotificationDTO { Read = true };

      
        Assert.IsTrue(notification.Read);  
    }

   


}
