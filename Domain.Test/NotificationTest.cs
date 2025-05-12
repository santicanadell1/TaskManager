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
            
            Notification not;
          
            not = new Notification(false, "");
        }

        [TestMethod]
        public void MarkRead_ShouldSetReadToTrue()
        {
            
            Notification not;
            not = new Notification(false, "Some description");

           
            not.MarkRead();

          
            Assert.IsTrue(not.Read);
        }
        
        [TestMethod]
        [ExpectedException(typeof(NotificationDescriptionException))]
        public void NewNotification_WhenDescriptionIsWhiteSpace_ShouldThrowNotificationDescriptionException()
        {
           
            Notification not = new Notification(false, "    ");
        }
        
        [TestMethod]
        public void NewNotification_WhenCreated_ShouldHaveReadAsFalse()
        {
            Notification not = new Notification(false, "Some description");

       
            Assert.IsFalse(not.Read);
        }

        [TestMethod]
        public void Description_WhenSetToValidValue_ThenNoExceptionThrown()
        {
            Notification not = new Notification(false, "Valid description");

           
            Assert.AreEqual("Valid description", not.Description);
        }

        [TestMethod]
        public void Description_WhenChanged_ThenUpdatedSuccessfully()
        {
            
            Notification not = new Notification(false, "Initial description");

 
            not.Description = "Updated description";

         
            Assert.AreEqual("Updated description", not.Description);
        }

        [TestMethod]
        public void Project_WhenSetIsNull_ThenNoExceptionThrown()
        {
            Project project = new Project();
            project.Name = "Project name";
            Notification not = new Notification(false, "");
            not.Id = 1;
            Notification.Project(project);
            
            Assert.AreEqual(project.Name, not.Project);
            
        }



        
    }
}