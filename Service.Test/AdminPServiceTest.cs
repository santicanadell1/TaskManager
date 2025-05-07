using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service.Models;
using DataAccess;
using Domain;
using System;
using System.Collections.Generic;

namespace Service.Test;

[TestClass]
public class AdminPServiceTests
{
    private InMemoryDatabase _database;
    private AdminPService _service;

    [TestInitialize]
    public void Setup()
    {
        _database = new InMemoryDatabase();
        _service = new AdminPService(_database);
    }

    [TestMethod]
    public void CreateProject_ShouldAddProjectToDatabase_WhenValid()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Parse("2021-09-01"),
        };

        _service.CreateProject(projectDTO);

        var project = _database.projects.GetProject(p => p.Name == projectDTO.Name);
        Assert.IsNotNull(project);
        Assert.AreEqual("New Project", project.Name);
    }

    [TestMethod]
    public void AssignMembersToProject_ShouldAddMembers_WhenValid()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now
        };

        _service.CreateProject(projectDTO);

        var project = _database.projects.GetProject(p => p.Name == projectDTO.Name);

        var userDTO = new UserDTO
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Birthday = DateTime.Parse("1990-01-01"),
            Password = "password123",
            Roles = new List<Rol> { Rol.AdminProject }
        };

        _service.AssignMembersToProject(project.Name, new List<UserDTO> { userDTO });

        Assert.IsTrue(project.Members.Count > 0);
        Assert.AreEqual("John", project.Members[0].FirstName);
    }

    [TestMethod]
    public void RemoveProject_ShouldRemoveProject_WhenValid()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "New Project",
            Description = "Project Description",
            StartDate = DateTime.Now
        };

        _service.CreateProject(projectDTO);

        var project = _database.projects.GetProject(p => p.Name == projectDTO.Name);
        Assert.IsNotNull(project);

        _service.RemoveProject("New Project");

        project = _database.projects.GetProject(p => p.Name == "New Project");
        Assert.IsNull(project);
    }


    [TestMethod]
    public void UpdateProject_ShouldUpdateProject_WhenValid()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Old Project",
            Description = "Old Description",
            StartDate = DateTime.Now
        };

        _service.CreateProject(projectDTO);

        var project = _database.projects.GetProject(p => p.Name == projectDTO.Name);
        Assert.IsNotNull(project);

        var updatedDTO = new ProjectDTO
        {
            Name = "Updated Project",
            Description = "Updated Description",
            StartDate = DateTime.Now.AddDays(1)
        };

        _service.UpdateProject("Old Project", updatedDTO);
    }
}