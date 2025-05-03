using System.ComponentModel.DataAnnotations;
using Service.Models;

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
    
    [TestMethod]
    public void ResourceDTO_ShouldFailValidation_WhenTypeIsNull()
    {
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = null,  
        };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(resourceDTO, new ValidationContext(resourceDTO), validationResults, true);

        Assert.IsFalse(isValid);  
        Assert.AreEqual(1, validationResults.Count);  
        Assert.AreEqual("LastName is required.", validationResults[0].ErrorMessage);  
    }
    
    [TestMethod]
    public void ResourceDTO_ShouldFailValidation_WhenDescriptionIsNull()
    {
        var resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = null  
        };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(resourceDTO, new ValidationContext(resourceDTO), validationResults, true);

        Assert.IsFalse(isValid);  
        Assert.AreEqual(1, validationResults.Count);  
        Assert.AreEqual("Description is required.", validationResults[0].ErrorMessage);  
    }

}