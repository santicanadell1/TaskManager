namespace DataAccess.Test;
using Domain;
using Exceptions.UserRepositoryExceptions;
using global::DataAccess;

[TestClass]
public class UserRepositoryTests
{
    [TestMethod]
    public void NewUserRepository_WhenRepositoryIsCreated_ShouldNotBeNull()
    {
        UserRepository userRepository;
        userRepository = new UserRepository();
        Assert.IsNotNull(userRepository);
    }
    [TestMethod]
    public void AddNewUser_WhenAddNewUser_ListShouldContainUser()
    {
        UserRepository userRepository = new UserRepository();
        User user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        userRepository.AddUser(user);
        Assert.IsTrue(userRepository.GetAll().Contains(user));
    }
    [TestMethod]
    [ExpectedException(typeof(UserEmailIsDuplicatedException))]
    public void AddNewUser_IfUserEmailAlreadyExists_ShouldThrowUseEmailIsDuplicatedException()
    {
        UserRepository userRepository = new UserRepository();
        User user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        User user2 = new User("First Name 2", "Last Name 2", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        userRepository.AddUser(user);
        userRepository.AddUser(user2);
    }
    [TestMethod]
    public void AddNewUser_WhenGettingAUser_ShouldReturnUser()
    {
        UserRepository userRepository = new UserRepository();
        User user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        User user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Parse("20/12/12"), "Password");
        userRepository.AddUser(user);
        userRepository.AddUser(user2);
        User user3 = userRepository.Get(u => u.Email == "Email1@email.com");
        Assert.AreEqual(user, user3);
    }

    [TestMethod]
    public void updateAUser_WhenGettingTheUser_ShouldBeDifferentFromTheOriginalUser()
    {
        UserRepository userRepository = new UserRepository();
        User user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        User user2 = new User("First Name 2", "Last Name 2", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        userRepository.AddUser(user);
        userRepository.Update(user.Email,user2);
        Assert.AreNotEqual(user, userRepository.Get(u => u.Email == "Email1@email.com"));
    }
    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void updateAUser_WhenEmailIsNotFound_ThrowUserNotFoundException()
    {
        UserRepository userRepository = new UserRepository();
        User user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        User user2 = new User("First Name 2", "Last Name 2", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        userRepository.AddUser(user);
        userRepository.Update("EmailDiferente@email.com",user2);
    }
    [TestMethod]
    [ExpectedException(typeof(UserEmailIsDuplicatedException))]
    public void updateAUser_WhenEmailAlreadyExists_ThrowUserEmailIsDuplicatedException()
    {
        UserRepository userRepository = new UserRepository();
        User user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        User user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Parse("20/12/12"), "Password");
        User user3 = new User("First Name 3", "Last Name 3", "Email2@email.com", DateTime.Parse("20/12/12"), "Password");
        userRepository.AddUser(user);
        userRepository.AddUser(user2);
        userRepository.Update("Email1@email.com",user3);
    }
    [TestMethod]
    public void DeleteAUser_WhenGettingTheUser_ShouldBeNull()
    {
        UserRepository userRepository = new UserRepository();
        User user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Parse("20/12/12"), "Password");
        User user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Parse("20/12/12"), "Password");
        userRepository.AddUser(user);
        userRepository.AddUser(user2);
        userRepository.Delete(user.Email);
        Assert.IsNull(userRepository.Get(u => u.Email == "Email1@email.com"));
    }
    
    
}