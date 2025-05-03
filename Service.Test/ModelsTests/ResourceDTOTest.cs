using System.ComponentModel.DataAnnotations;

namespace Service.Test.ModelsTests;

[TestClass]
public class ResourceDTOTest
{
    [TestMethod]
    public void ResourceDTO_ShouldFailValidation_WhenNameIsNull()
    {
        var resourceDTO = new ResourceDTO
        {
            Name = null,  
            
        };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(resourceDTO, new ValidationContext(resourceDTO), validationResults, true);

        Assert.IsFalse(isValid);  
        Assert.AreEqual(1, validationResults.Count);  
        Assert.AreEqual("Name is required.", validationResults[0].ErrorMessage);  
    }
}