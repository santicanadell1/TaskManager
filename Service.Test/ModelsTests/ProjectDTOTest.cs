using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service.Models;

namespace Service.Test.ModelsTests;

[TestClass]
public class ProjectDTOTests
{
    private UserDTO Admin;
    private UserDTO Leader;
    private List<UserDTO> members;
    private UserDTO User;

    [TestInitialize]
    public void Initialize()
    {
        Admin = new UserDTO
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin.user@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<RolDTO> { RolDTO.AdminProject }
        };
        User = new UserDTO
        {
            FirstName = "User",
            LastName = "Member",
            Email = "member.user@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "Password123@",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };
        Leader = new UserDTO
        {
            FirstName = "Leader",
            LastName = "Member",
            Email = "leader.user@example.com",
            Birthday = DateTime.Parse("1985-05-01"),
            Password = "Password123@",
            Roles = new List<RolDTO> { RolDTO.ProjectMember }
        };
        members = new List<UserDTO> { User };
    }

    [TestMethod]
    public void Validate_ShouldThrowValidationException_WhenNameIsNullOrWhiteSpace()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "",
            Description = "Test Description",
            StartDate = DateTime.Now,
            AdminProyect = Admin,
            Members = members,
            ProjectLeader = Leader
        };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            projectDTO,
            new ValidationContext(projectDTO),
            validationResults,
            true
        );

        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void Validate_ShouldThrowValidationException_WhenDescriptionIsNullOrWhiteSpace()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "",
            StartDate = DateTime.Now,
            AdminProyect = Admin,
            Members = members,
            ProjectLeader = Leader
        };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            projectDTO,
            new ValidationContext(projectDTO),
            validationResults,
            true
        );

        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void Validate_ShouldThrowValidationException_WhenLeaderIsNull()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Now,
            AdminProyect = Admin,
            Members = members,
            ProjectLeader = null
        };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            projectDTO,
            new ValidationContext(projectDTO),
            validationResults,
            true
        );

        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public void Validate_ShouldNotThrowValidationException_WhenAllFieldsAreValid()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Parse("2020-09-01"),
            AdminProyect = Admin,
            Members = members,
            ProjectLeader = Leader
        };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            projectDTO,
            new ValidationContext(projectDTO),
            validationResults,
            true
        );

        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public void Validate_AtributesMustBeValid()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Parse("2020-09-01"),
            AdminProyect = Admin,
            Members = members,
            ProjectLeader = Leader
        };

        Assert.IsTrue(
            projectDTO.StartDate == DateTime.Parse("2020-09-01") &&
            projectDTO.Name == "Test Project" &&
            projectDTO.Description == "Test Description" &&
            projectDTO.AdminProyect == Admin &&
            projectDTO.Members == members &&
            projectDTO.ProjectLeader == Leader
        );
    }
}