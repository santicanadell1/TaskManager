using Domain.Exceptions.NotificationExceptions;

namespace Domain.Test
{
    [TestClass]
    public class ResourceTest
    {
        [TestMethod]
        [ExpectedException(typeof(ResourceNameException))]
        public void NewResource_WhenNameIsEmpty_ThenThrowResourceNameException()
        {
            Resource res;

            res = new Resource("", "type", "description");
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceTypeException))]
        public void NewResource_WhenTypeIsEmpty_ThenThrowResourceTypeException()
        {
            Resource res;

            res = new Resource("name", "", "description");
        }

        [TestMethod]
        public void NewResource_WithValidValues_ShouldCreateResourceCorrectly()
        {
            var name = "Laptop";
            var type = "Hardware";
            var description = "Dell Latitude";


            var res = new Resource(name, type, description);


            Assert.AreEqual(name, res.Name);
            Assert.AreEqual(type, res.Type);
            Assert.AreEqual(description, res.Description);
        }

        [TestMethod]
        public void UpdateResourceName_ShouldChangeNameSuccessfully()
        {
            var res = new Resource("Old Name", "Humano", "Some description");
            

            res.Name = "New Name";
            res.Id = 1;


            Assert.AreEqual("New Name", res.Name);
        }
    }
}