namespace DataAccess.Test;

[TestClass]
public class ProjectRepositoryTest
{
    [TestMethod]
    public void NewProjectRepository_WhenRepositoryIsCreated_ShouldNotBeNull()
    {
        ProjectRepository projectRepository;
        projectRepository = new ProjectRepository();
        Assert.IsNotNull(projectRepository);
    }
}