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
    [TestMethod]
    public void NewNotification_WhenReadIsFalse_ThenNotificationIsCreatedWithFalse()
    {
    
        var notification = new NotificationDTO { Read = false };

   
        Assert.IsFalse(notification.Read);  
    }

    [TestMethod]
    public void NewNotification_WhenReadIsNotSet_ThenNotificationIsNotCreated()
    {
       
        var notification = new NotificationDTO(); 

     
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(notification);
        var isValid = Validator.TryValidateObject(notification, validationContext, validationResults, true);


        Assert.IsFalse(isValid); 
        Assert.IsTrue(validationResults.Any(v => v.ErrorMessage.Contains("Read is required.")));
    }




}
