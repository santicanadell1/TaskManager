using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Service.Test.ModelsTests;

[TestClass]
public class ProjectDTOTests
{
    [TestMethod]
    public void Validate_ShouldThrowValidationException_WhenNameIsNullOrWhiteSpace()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "",
            Description = "Test Description",
            StartDate = DateTime.Now
        };

        var validationResults = new System.Collections.Generic.List<ValidationResult>();
        bool isValid =
            Validator.TryValidateObject(projectDTO, new ValidationContext(projectDTO), validationResults, true);

        Assert.IsFalse(isValid);
    }


    [TestMethod]
    public void Validate_ShouldThrowValidationException_WhenDescriptionIsNullOrWhiteSpace()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "",
            StartDate = DateTime.Now
        };

        var validationResults = new System.Collections.Generic.List<ValidationResult>();
        bool isValid =
            Validator.TryValidateObject(projectDTO, new ValidationContext(projectDTO), validationResults, true);

        Assert.IsFalse(isValid);
    }


    [TestMethod]
    public void Validate_ShouldNotThrowValidationException_WhenAllFieldsAreValid()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Parse("")
        };

        var validationResults = new System.Collections.Generic.List<ValidationResult>();
        bool isValid =
            Validator.TryValidateObject(projectDTO, new ValidationContext(projectDTO), validationResults, true);

        Assert.IsTrue(isValid);
    }
}