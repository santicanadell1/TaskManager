namespace DataAccess.Test;

[TestClass]
public class UserRepositoryTest
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Add_WhenAddIsInvokedWithAnEmptyUser_ThenThrowException()
    {
        //arrange
        //act
        _userRepository.Add(new User());
    }
}