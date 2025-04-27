namespace Service.Test;
[TestClass]
public class LoggedUserTest
{
    [TestMethod]
    public void LoggedUser_WhenCurrentIsAssignedNull_ThenCurrentIsNull()
    {
        // act
        LoggedUser.Current = null;

        // assert
        Assert.IsNull(LoggedUser.Current);
    }
}