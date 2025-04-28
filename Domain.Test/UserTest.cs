using Domain.Exceptions;

namespace Domain.Test
{
    [TestClass]
    public class UserTest
    {
        [TestMethod]
        public void NewUser_WhenConstructorIsNotEmpty_ThenUserIsCreated()
        {
            // arrange
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            // act
            user = new User("First Name", "Last Name", "Email@email.com", birthday, "Password");
            // assert
            Assert.IsNotNull(user);
        }

        [TestMethod]
        [ExpectedException(typeof(UserFirstNameException))]
        public void NewUser_WhenFirstNameIsNull_ThenThrowUserFirstNameException()
        {
            // arrange
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            // act
            user = new User("  ", "Last Name", "Email@email.com", birthday, "Password");
        }

        [TestMethod]
        [ExpectedException(typeof(UserLastNameException))]
        public void NewUser_WhenLastNameIsNull_ThenThrowUserLastNameException()
        {
            // arrange
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            // act
            user = new User("First Name", "", "Email@email.com", birthday, "Password");
        }

        [TestMethod]
        [ExpectedException(typeof(UserEmailException))]
        public void NewUser_WhenEmailIsNull_ThenThrowUserEmailException()
        {
            // arrange
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            // act
            user = new User("First Name", "Last Name", "", birthday, "Password");
        }

        [TestMethod]
        [ExpectedException(typeof(UserEmailException))]
        public void NewUser_WhenEmailHasAnInvalidFormat_ThenThrowUserEmailException()
        {
            // arrange
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            // act
            user = new User("First Name", "Last Name", "email", birthday, "Password");
        }

        [TestMethod]
        public void NewUser_WhenEmailIsValid_ThenUserIsCreated()
        {
            // arrange
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            // act
            user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");
            // assert
            Assert.IsNotNull(user);
        }

        [TestMethod]
        [ExpectedException(typeof(UserBirthdayException))]
        public void NewUser_WhenDateIsAfterToday_ThenThrowUserBirthdayException()
        {
            // arrange
            User user;
            DateTime birthday = DateTime.Parse("20/07/2026");
            // act
            user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");
        }

        [TestMethod]
        [ExpectedException(typeof(UserPasswordException))]
        public void NewUser_WhenPasswordIsNull_ThenThrowUserPasswordException()
        {
            // arrange
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            // act
            user = new User("First Name", "Last Name", "Email@email.com", birthday, "");
        }

        [TestMethod]
        public void AddRol_WhenRoleIsAdded_ThenRoleIsInList()
        {
            // Arrange
            var roles = new List<Rol> { Rol.AdminSystem };
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");

            // Act
            user.Roles = roles;
            user.AddRol(Rol.ProjectMember);

            // Assert
            Assert.AreEqual(2, user.Roles.Count);
            Assert.IsTrue(user.Roles.Contains(Rol.ProjectMember));
        }

        [TestMethod]
        [ExpectedException(typeof(UserRoleAlreadyExistsException))]
        public void AddRol_WhenRoleAlreadyExists_ThenThrowUserRoleAlreadyExistsException()
        {
            // Arrange
            var roles = new List<Rol> { Rol.AdminSystem };
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");

            // Act
            user.Roles = roles;
            user.AddRol(Rol.AdminSystem); 
        }

        [TestMethod]
        [ExpectedException(typeof(UserRoleNotFoundException))]
        public void RemoveRol_WhenRoleDoesNotExist_ThenThrowUserRoleNotFoundException()
        {
            // Arrange: 
            var roles = new List<Rol> { Rol.AdminSystem, Rol.AdminProject };
            User user;
            DateTime birthday = DateTime.Parse("10/05/2005");
            user = new User("First Name", "Last Name", "email@email.com", birthday, "Password");

            // Act: 
            user.Roles = roles;
            user.RemoveRol(Rol.AdminProject);
            user.RemoveRol(Rol.ProjectMember);
        }

        [TestMethod]
        public void User_WhenInitializedWithEmptyConstructor_ThenPropertiesAreInitializedWithDefaultValues()
        {
            // Arrange
            User user;

            // Act
            user = new User();
        }
    }
}