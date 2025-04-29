using Domain.Exceptions;

namespace Domain.Test
{
    [TestClass]
    public class ProjectTest
    {
        [TestMethod]
        public void GivenProject_WhenNameIsSet_ThenNameShouldBeCorrect()
        {
          
            var project = new Project();
            string expectedName = "Project A";

          
            project.Name = expectedName;
            string actualName = project.Name;

           
            Assert.AreEqual(expectedName, actualName);
        }

        [TestMethod]
        public void GivenProject_WhenDescriptionIsSet_ThenDescriptionShouldBeCorrect()
        {
        
            var project = new Project();
            string expectedDescription = "This is a test project";

            
            project.Description = expectedDescription;
            string actualDescription = project.Description;

            
            Assert.AreEqual(expectedDescription, actualDescription);
        }

        [TestMethod]
        public void GivenProject_WhenStartDateIsSet_ThenStartDateShouldBeCorrect()
        {
        
            var project = new Project();
            DateTime expectedStartDate = new DateTime(2025, 5, 1);

         
            project.StartDate = expectedStartDate;
            DateTime actualStartDate = project.StartDate;

         
            Assert.AreEqual(expectedStartDate, actualStartDate);
        }

        [TestMethod]
        public void GivenProject_WhenMembersAreSet_ThenMembersShouldBeCorrect()
        {
          
            var project = new Project();
            var expectedMembers = new List<User>();

        
            project.Members = expectedMembers;
            List<User> actualMembers = project.Members;

          
            CollectionAssert.AreEqual(expectedMembers, actualMembers);
        }

        [TestMethod]
        public void GivenProject_WhenNotificationsAreSet_ThenNotificationsShouldBeCorrect()
        {
            var project = new Project();
            var expectedNotifications = new List<Notification>();
            
            project.Notifications = expectedNotifications;
            List<Notification> actualNotifications = project.Notifications;
            
            CollectionAssert.AreEqual(expectedNotifications, actualNotifications);
        }
        

    [TestMethod]
        public void GivenTaskList_WhenTasksAreAdded_ThenTaskListShouldContainCorrectTasks()
        {
       
            var project = new Project();
            var expectedTasks = new List<Task>();
            
       
            project.Tasks = expectedTasks; 
            var actualTasks = project.Tasks; 

     
            CollectionAssert.AreEqual(expectedTasks, actualTasks); 
        }

        [TestMethod]
        public void GivenProject_WhenAdminProjectIsSet_ThenAdminProjectShouldBeCorrect()
        {
           
            var project = new Project();
            var adminUser = new User();
            adminUser.Roles = new List<Rol>(); 
            adminUser.AddRol(Rol.AdminProject);  

        
            project.AdminProject = adminUser;
            var actualAdminProject = project.AdminProject;

  
            Assert.IsTrue(actualAdminProject.Roles.Contains(Rol.AdminProject)); 
        }

        [TestMethod]
        [ExpectedException(typeof(ProjectNameException))]
        public void GivenProject_WhenNameIsNull_ThenProjectNameExceptionShouldBeThrown()
        {
       
            var project = new Project();
    
   
            project.Name = null; 
        }
        
        [TestMethod]
        [ExpectedException(typeof(ProjectDescriptionException))]
        public void GivenProject_WhenDescriptionIsEmpty_ThenProjectDescriptionExceptionShouldBeThrown()
        {
        
            var project = new Project();
    
        
            project.Description = "";  
        }
        
        [TestMethod]
        [ExpectedException(typeof(ProjectStartDateException))]
        public void GivenProject_WhenStartDateIsDefault_ThenProjectStartDateExceptionShouldBeThrown()
        {
        
            var project = new Project();
    
      
            project.StartDate = default(DateTime);  
        }
    }
}
