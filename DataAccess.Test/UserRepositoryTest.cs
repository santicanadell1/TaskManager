using Domain;

namespace DataAccess.Test;

[TestClass]
public class UserRepositoryTest
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Add_WhenAddIsInvokedWithAnEmptyUser_ThenThrowException()
    {
        //arrange
        DataAccess dataAccess = null; ;
        UserRepository _userRepository = new UserRepository(dataAccess);
        User user = null;
        //act
        _userRepository.Add(user);
    }
}