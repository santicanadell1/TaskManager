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
    
    
}