using System.ComponentModel.DataAnnotations;
using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class ResourceDTOTest
{
    [TestMethod]
    public void ResourceDTO_ShouldFailValidation_WhenNameIsNull()
    {
        ResourceDTO resourceDTO = new ResourceDTO
        {
            Name = null
        };

        var validationResults = new List<ValidationResult>();
        bool isValid =
            Validator.TryValidateObject(resourceDTO, new ValidationContext(resourceDTO), validationResults, true);

        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void ResourceDTO_ShouldFailValidation_WhenTypeIsNull()
    {
        ResourceDTO resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = null
        };

        var validationResults = new List<ValidationResult>();
        bool isValid =
            Validator.TryValidateObject(resourceDTO, new ValidationContext(resourceDTO), validationResults, true);

        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void ResourceDTO_ShouldFailValidation_WhenDescriptionIsNull()
    {
        ResourceDTO resourceDTO = new ResourceDTO
        {
            Name = "Resource1",
            Type = "TypeA",
            Description = null
        };

        var validationResults = new List<ValidationResult>();
        bool isValid =
            Validator.TryValidateObject(resourceDTO, new ValidationContext(resourceDTO), validationResults, true);

        Assert.IsFalse(isValid);
    }
}