using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;

namespace DataAccess.Test;

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
        var userRepository = new UserRepository();
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        userRepository.AddUser(user);
        Assert.IsTrue(userRepository.GetAll().Contains(user));
    }

    [TestMethod]
    [ExpectedException(typeof(UserEmailIsDuplicatedException))]
    public void AddNewUser_IfUserEmailAlreadyExists_ShouldThrowUseEmailIsDuplicatedException()
    {
        var userRepository = new UserRepository();
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        userRepository.AddUser(user);
        userRepository.AddUser(user2);
    }

    [TestMethod]
    public void AddNewUser_WhenGettingAUser_ShouldReturnUser()
    {
        var userRepository = new UserRepository();
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Today.AddYears(-18),
            "Password");
        userRepository.AddUser(user);
        userRepository.AddUser(user2);
        var user3 = userRepository.Get(u => u.Email == "Email1@email.com");
        Assert.AreEqual(user, user3);
    }

    [TestMethod]
    public void updateAUser_WhenGettingTheUser_ShouldBeDifferentFromTheOriginalUser()
    {
        var userRepository = new UserRepository();
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        userRepository.AddUser(user);
        userRepository.Update(user.Email, user2);
        Assert.AreNotEqual(user, userRepository.Get(u => u.Email == "Email1@email.com"));
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void updateAUser_WhenEmailIsNotFound_ThrowUserNotFoundException()
    {
        var userRepository = new UserRepository();
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        userRepository.AddUser(user);
        userRepository.Update("EmailDiferente@email.com", user2);
    }

    [TestMethod]
    [ExpectedException(typeof(UserEmailIsDuplicatedException))]
    public void updateAUser_WhenEmailAlreadyExists_ThrowUserEmailIsDuplicatedException()
    {
        var userRepository = new UserRepository();
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Today.AddYears(-18),
            "Password");
        var user3 = new User("First Name 3", "Last Name 3", "Email2@email.com", DateTime.Today.AddYears(-18),
            "Password");
        userRepository.AddUser(user);
        userRepository.AddUser(user2);
        userRepository.Update("Email1@email.com", user3);
    }

    [TestMethod]
    public void DeleteAUser_WhenGettingTheUser_ShouldBeNull()
    {
        var userRepository = new UserRepository();
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Today.AddYears(-18),
            "Password");
        userRepository.AddUser(user);
        userRepository.AddUser(user2);
        userRepository.Delete(user.Email);
        Assert.IsNull(userRepository.Get(u => u.Email == "Email1@email.com"));
    }
}