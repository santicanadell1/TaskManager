using Service.Models;

namespace Service.Test;

[TestClass]
public class LoggedUserTest
{
    [TestMethod]
    public void LoggedUser_WhenCurrentIsAssignedNull_ThenCurrentIsNull()
    {
        LoggedUser.Current = null;
        Assert.IsNull(LoggedUser.Current);
    }
}