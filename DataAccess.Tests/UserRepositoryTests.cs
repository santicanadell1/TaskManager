using DataAccess.Exceptions.UserRepositoryExceptions;
using Domain;

namespace DataAccess.Test;

[TestClass]
public class UserRepositoryTests
{
    private UserRepository _userRepository;
    private AppDbContext _context;
    private InMemoryAppContextFactory _contextFactory;

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
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18), "Password");
        _userRepository.Add(user);
        _context.SaveChanges();
        Assert.IsTrue(_userRepository.GetAll().Contains(user));
    }

    [TestMethod]
    [ExpectedException(typeof(UserEmailIsDuplicatedException))]
    public void AddNewUser_IfUserEmailAlreadyExists_ShouldThrowUseEmailIsDuplicatedException()
    {
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18), "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email1@email.com", DateTime.Today.AddYears(-18), "Password");
        _userRepository.Add(user);
        _userRepository.Add(user2);
        _context.SaveChanges();
    }

    [TestMethod]
    public void AddNewUser_WhenGettingAUser_ShouldReturnUser()
    {
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18), "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Today.AddYears(-18), "Password");
        _userRepository.Add(user);
        _userRepository.Add(user2);
        _context.SaveChanges();

        var user3 = _userRepository.Get(u => u.Email == "Email1@email.com");
        Assert.AreEqual(user, user3);
    }

    [TestMethod]
    public void UpdateAUser_WhenGettingTheUser_ShouldBeDifferentFromTheOriginalUser()
    {
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18), "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email1@email.com", DateTime.Today.AddYears(-18), "Password");
        _userRepository.Add(user);
        _userRepository.Update(user2);

        Assert.AreNotEqual("First Name 1", _userRepository.Get(u => u.Email == "Email1@email.com").FirstName);
    }

    [TestMethod]
    [ExpectedException(typeof(UserNotFoundException))]
    public void UpdateAUser_WhenEmailIsNotFound_ThrowUserNotFoundException()
    {
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18), "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Today.AddYears(-18), "Password");
        _userRepository.Add(user);
        _userRepository.Update(user2);
    }
    
    [TestMethod]
    public void DeleteAUser_WhenGettingTheUser_ShouldBeNull()
    {
        var user = new User("First Name 1", "Last Name 1", "Email1@email.com", DateTime.Today.AddYears(-18), "Password");
        var user2 = new User("First Name 2", "Last Name 2", "Email2@email.com", DateTime.Today.AddYears(-18), "Password");
        _userRepository.Add(user);
        _userRepository.Add(user2);
        _context.SaveChanges();

        _userRepository.Delete(user);
        _context.SaveChanges();

        Assert.IsNull(_userRepository.Get(u => u.Email == "Email1@email.com"));
    }
}
