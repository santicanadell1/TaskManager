using Domain.Exceptions;

namespace Domain.Test
{
    [TestClass]
    public class UserTest
    {
        [TestMethod]
        public void NewUser_WhenConstructorIsNotEmpty_ThenUserIsCreated()
        {
            User user;
            DateTime birthday = DateTime.Parse("10/03/2005");

            user = new User("First Name", "Last Name", "Email@email.com", birthday, "Password");

            Assert.IsNotNull(user);
        }

        [TestMethod]
        [ExpectedException(typeof(UserFirstNameException))]
        public void NewUser_WhenFirstNameIsNull_ThenThrowUserFirstNameException()
        {
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");

            user = new User("  ", "Last Name", "Email@email.com", birthday, "Password");
        }

        [TestMethod]
        [ExpectedException(typeof(UserLastNameException))]
        public void NewUser_WhenLastNameIsNull_ThenThrowUserLastNameException()
        {
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");

            user = new User("First Name", "", "Email@email.com", birthday, "Password");
        }

        [TestMethod]
        [ExpectedException(typeof(UserEmailException))]
        public void NewUser_WhenEmailIsNull_ThenThrowUserEmailException()
        {
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");

            user = new User("First Name", "Last Name", "", birthday, "Password");
        }

        [TestMethod]
        [ExpectedException(typeof(UserEmailException))]
        public void NewUser_WhenEmailHasAnInvalidFormat_ThenThrowUserEmailException()
        {
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            user = new User("First Name", "Last Name", "email", birthday, "Password");
        }

        [TestMethod]
        public void NewUser_WhenEmailIsValid_ThenUserIsCreated()
        {
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");

            user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");

            Assert.IsNotNull(user);
        }

        [TestMethod]
        [ExpectedException(typeof(UserBirthdayException))]
        public void NewUser_WhenDateIsAfterToday_ThenThrowUserBirthdayException()
        {
            User user;
            DateTime birthday = DateTime.Parse("20/07/2026");

            user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");
        }

        [TestMethod]
        [ExpectedException(typeof(UserPasswordException))]
        public void NewUser_WhenPasswordIsNull_ThenThrowUserPasswordException()
        {
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");

            user = new User("First Name", "Last Name", "Email@email.com", birthday, "");
        }

        [TestMethod]
        public void AddRol_WhenRoleIsAdded_ThenRoleIsInList()
        {
            var roles = new List<Rol> { Rol.AdminSystem };
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");

            user.Roles = roles;
            user.AddRol(Rol.ProjectMember);

            Assert.AreEqual(2, user.Roles.Count);
            Assert.IsTrue(user.Roles.Contains(Rol.ProjectMember));
        }

        [TestMethod]
        [ExpectedException(typeof(UserRoleAlreadyExistsException))]
        public void AddRol_WhenRoleAlreadyExists_ThenThrowUserRoleAlreadyExistsException()
        {
            var roles = new List<Rol> { Rol.AdminSystem };
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");

            user.Roles = roles;
            user.AddRol(Rol.AdminSystem); 
        }

        [TestMethod]
        [ExpectedException(typeof(UserRoleNotFoundException))]
        public void RemoveRol_WhenRoleDoesNotExist_ThenThrowUserRoleNotFoundException()
        {
            var roles = new List<Rol> { Rol.AdminSystem, Rol.AdminProject };
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");

            user.Roles = roles;
            user.RemoveRol(Rol.AdminProject);
            user.RemoveRol(Rol.ProjectMember);
        }

        [TestMethod]
        public void User_WhenInitializedWithEmptyConstructor_ThenPropertiesAreInitializedWithDefaultValues()
        {
           
            User user = new User();

        
            Assert.IsNull(user.FirstName);
            Assert.IsNull(user.LastName);
            Assert.IsNull(user.Email);
            Assert.IsNull(user.Password);
            Assert.AreEqual(0, user.Roles.Count);  
        }


        [TestMethod]
        public void FirstName_WhenValid_ThenUserIsCreated()
        {
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");

            user = new User("John", "Doe", "email@email.com", birthday, "Password");

            Assert.AreEqual("John", user.FirstName);
        }
        
        [TestMethod]
        [ExpectedException(typeof(UserLastNameException))]
        public void LastName_WhenInvalid_ThenThrowUserLastNameException()
        {
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");

            user = new User("John", "", "email@email.com", birthday, "Password");
        }
        
        [TestMethod]
        public void Email_WhenValid_ThenUserIsCreated()
        {
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");

            user = new User("John", "Doe", "john.doe@example.com", birthday, "Password");

            Assert.IsTrue(user.Email == "john.doe@example.com");
        }
        
        [TestMethod]
        public void Roles_WhenAssignedEmptyList_ThenRolesShouldBeEmpty()
        {
            User user = new User("John", "Doe", "email@email.com", DateTime.Parse("10/05/2005"), "Password");
            
            user.Roles = new List<Rol>();

            Assert.AreEqual(0, user.Roles.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(UserEmailException))]
        public void Email_WhenSetToInvalidFormat_ThenThrowUserEmailException()
        {
            User user = new User("John", "Doe", "valid.email@example.com", DateTime.Parse("10/05/2005"), "Password");

           
            user.Email = "invalidemail";
        }

        
        
        [TestMethod]
        public void AddRol_WhenRolesListIsEmpty_ThenRoleIsAdded()
        {
            User user = new User("John", "Doe", "email@email.com", DateTime.Parse("10/05/2005"), "Password");

            
            user.Roles = new List<Rol>();
            
            user.AddRol(Rol.AdminSystem);

            Assert.AreEqual(1, user.Roles.Count);
            Assert.IsTrue(user.Roles.Contains(Rol.AdminSystem));
        }


        
    
        [TestMethod]
        public void RemoveRol_WhenSingleRole_ThenRoleIsRemoved()
        {
            User user = new User("John", "Doe", "email@email.com", DateTime.Parse("10/05/2005"), "Password");

            
            user.Roles = new List<Rol> { Rol.AdminSystem };

   
            user.RemoveRol(Rol.AdminSystem);

            Assert.AreEqual(0, user.Roles.Count);
        }
        
        
        [TestMethod]
        public void NewUser_WhenAddingTaskID_ThenTaskIdIsAdded()
        {
            User user = new User("John", "Doe", "email@email.com", DateTime.Parse("10/05/2005"), "Password",new List<int> ());
            user.AddTask(1);
            
            Assert.AreEqual(1,user.Tasks.Count);
        }






   


        
    }
}