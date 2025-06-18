using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;

namespace DataAccess.Test;

[TestClass]
public class UserRepositoryTests
{
    private AppDbContext _context;
    private InMemoryAppContextFactory _contextFactory;
    private UserRepository _userRepository;

    [TestInitialize]
    public void Setup()
    {
        _contextFactory = new InMemoryAppContextFactory();
        _context = _contextFactory.CreateDbContext();
        _userRepository = new UserRepository(_context);
    }

    [TestCleanup]
    public void CleanUp()
    {
        _context.Database.EnsureDeleted();
    }

    [TestMethod]
    public void NewUserRepository_WhenRepositoryIsCreated_ShouldNotBeNull()
    {
        Assert.IsNotNull(_userRepository);
    }

    [TestMethod]
    public void AddNewUser_WhenAddNewUser_ListShouldContainUser()
    {
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        _userRepository.Add(user);
        _context.SaveChanges();
        Assert.IsTrue(_userRepository.GetAll().Contains(user));
    }

    [TestMethod]
    [ExpectedException(typeof(UserEmailIsDuplicatedException))]
    public void AddNewUser_IfUserEmailAlreadyExists_ShouldThrowUseEmailIsDuplicatedException()
    {
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        _userRepository.Add(user);
        _userRepository.Add(user2);
        _context.SaveChanges();
    }

    [TestMethod]
    public void AddNewUser_WhenGettingAUser_ShouldReturnUser()
    {
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Today.AddYears(-18),
            "Password");
        _userRepository.Add(user);
        _userRepository.Add(user2);
        _context.SaveChanges();

        var user3 = _userRepository.Get(u => u.Email == "Email1@email.com");
        Assert.AreEqual(user, user3);
    }

    [TestMethod]
    public void UpdateAUser_WhenGettingTheUser_ShouldBeDifferentFromTheOriginalUser()
    {
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        _userRepository.Add(user);

        var addedUser = _userRepository.Get(u => u.Email == "Email1@email.com");

        var user2 = new User("First Name 2", "Last Name 2", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        user2.Id = addedUser.Id;

        _userRepository.Update(user2);

        Assert.AreNotEqual("First Name 1", _userRepository.Get(u => u.Email == "Email1@email.com").FirstName);
        Assert.AreEqual("First Name 2", _userRepository.Get(u => u.Email == "Email1@email.com").FirstName);
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void UpdateAUser_WhenEmailIsNotFound_ThrowUserNotFoundException()
    {
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Today.AddYears(-18),
            "Password");
        _userRepository.Add(user);
        _userRepository.Update(user2);
    }

    [TestMethod]
    public void DeleteAUser_WhenGettingTheUser_ShouldBeNull()
    {
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18),
            "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Today.AddYears(-18),
            "Password");
        _userRepository.Add(user);
        _userRepository.Add(user2);
        _context.SaveChanges();

        _userRepository.Delete(user);
        _context.SaveChanges();

        Assert.IsNull(_userRepository.Get(u => u.Email == "Email1@email.com"));
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void Add_ShouldThrowUserNotFoundException_WhenUserIsNull()
    {
        _userRepository.Add(null);
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void Update_ShouldThrowUserNotFoundException_WhenUserIsNull()
    {
        _userRepository.Update(null);
    }

    [TestMethod]
    public void Update_ShouldUpdateAllUserProperties_WhenUserExists()
    {
        var originalUser = new User("Original", "User", "original@email.com", DateTime.Today.AddYears(-20),
            "OriginalPassword");
        _userRepository.Add(originalUser);

        var addedUser = _userRepository.Get(u => u.Email == "original@email.com");
        var updatedUser = new User("Updated", "Name", "original@email.com", DateTime.Today.AddYears(-25),
            "UpdatedPassword");
        updatedUser.Id = addedUser.Id;

        _userRepository.Update(updatedUser);

        var retrievedUser = _userRepository.Get(u => u.Id == updatedUser.Id);
        Assert.AreEqual("Updated", retrievedUser.FirstName);
        Assert.AreEqual("Name", retrievedUser.LastName);
        Assert.AreEqual("UpdatedPassword", retrievedUser.Password);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void Delete_ShouldThrowException_WhenUserDoesNotExist()
    {
        var nonExistentUser = new User("Non", "Existent", "nonexistent@email.com", DateTime.Today.AddYears(-18),
            "Password");

        _userRepository.Delete(nonExistentUser);
    }

    [TestMethod]
    public void Delete_ShouldClearNotificationsAndTasks_WhenUserHasRelatedData()
    {
        var user = new User("Test", "User", "test@email.com", DateTime.Today.AddYears(-18), "Password");
        _userRepository.Add(user);

        var addedUser = _userRepository.Get(u => u.Email == "test@email.com");
        Assert.IsNotNull(addedUser);

        _userRepository.Delete(addedUser);

        var deletedUser = _userRepository.Get(u => u.Email == "test@email.com");
        Assert.IsNull(deletedUser);
    }

    [TestMethod]
    public void ValidateDuplicateEmail_ShouldWork_WhenEmailDoesNotExist()
    {
        var user1 = new User("First", "User", "first@email.com", DateTime.Today.AddYears(-18), "Password");
        var user2 = new User("Second", "User", "second@email.com", DateTime.Today.AddYears(-18), "Password");

        _userRepository.Add(user1);
        _userRepository.Add(user2);

        var users = _userRepository.GetAll();
        Assert.AreEqual(2, users.Count);
        Assert.IsTrue(users.Any(u => u.Email == "first@email.com"));
        Assert.IsTrue(users.Any(u => u.Email == "second@email.com"));
    }
}