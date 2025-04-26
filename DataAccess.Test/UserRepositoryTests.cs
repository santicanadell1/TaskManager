namespace DataAccess.Test;
using DataAccess;
using Domain;

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
        userRepository.addUser(user);
        // Assert
        userRepository.getAll.contains(user);
    }
    
}