namespace DataAccess.Test;

using Domain;
using global::DataAccess;

[TestClass]
public class UserRepositoryTests
{
    [TestMethod]
    public void NewUserRepository_WhenRepositoryIsCreated_ShouldNotBeNull()
    {
        // Arrange
        UserRepository userRepository;
        // Act
        userRepository = new UserRepository();
        // Assert
        Assert.IsNotNull(userRepository);
    }
    [TestMethod]
    public void AddNewUser_WhenAddNewUser_ListShouldContainUser()
    {
        // Arrange
        UserRepository userRepository = new UserRepository();
        User user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        // Act
        userRepository.AddUser(user);
        // Assert
        Assert.IsTrue(userRepository.GetAll().Contains(user));
    }
    [TestMethod]
    public void AddNewUser_WhenGettingAUser_ShouldReturnUser()
    {
        // Arrange
        UserRepository userRepository = new UserRepository();
        User user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        User user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Parse("20/12/12"), "Password");
        userRepository.AddUser(user);
        userRepository.AddUser(user2);
        // Act
        User user3 = userRepository.Get(u => u.Email == "Email1@email.com");
        // Assert
        Assert.AreEqual(user, user3);
    }

    [TestMethod]
    public void updateAUser_WhenGettingTheUser_ShouldBeDifferentFromTheOriginalUser()
    {
        //Arrange
        UserRepository userRepository = new UserRepository();
        User user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        User user2 = new User("First Name 2", "Last Name 2", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        userRepository.AddUser(user);
        //Act
        userRepository.Update(user.Email,user2);
        //Assert
        Assert.AreNotEqual(user, userRepository.Get(u => u.Email == "Email1@email.com"));
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void updateAUser_WhenEmailIsNotFound_ThrowArgumentException()
    {
        //Arrange
        UserRepository userRepository = new UserRepository();
        User user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        User user2 = new User("First Name 2", "Last Name 2", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        userRepository.AddUser(user);
        //Act
        userRepository.Update("EmailDiferente@email.com",user2);
    }
    [TestMethod]
    public void DeleteAUser_WhenGettingTheUser_ShouldBeNull()
    {
        //Arrange
        UserRepository userRepository = new UserRepository();
        User user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        User user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Parse("20/12/12"), "Password");
        userRepository.AddUser(user);
        userRepository.AddUser(user2);
        //Act
        userRepository.Delete(user.Email);
        //Assert
        Assert.IsNull(userRepository.Get(u => u.Email == "Email1@email.com"));
    }
    
    
}