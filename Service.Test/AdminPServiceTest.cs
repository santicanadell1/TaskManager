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

    [TestMethod]
    public void GetAllProjects_ShouldReturnAllProjects()
    {
        var projectDTO1 = new ProjectDTO
        {
            Name = "Project 1",
            Description = "Description 1",
            StartDate = DateTime.Now
        };
        var projectDTO2 = new ProjectDTO
        {
            Name = "Project 2",
            Description = "Description 2",
            StartDate = DateTime.Now.AddDays(1)
        };

        _service.CreateProject(projectDTO1);
        _service.CreateProject(projectDTO2);

        var projects = _service.GetAllProjects();

        Assert.AreEqual(2, projects.Count);
        Assert.AreEqual("Project 1", projects[0].Name);
        Assert.AreEqual("Project 2", projects[1].Name);
    }

    [TestMethod]
    public void GetProjectByName_ShouldReturnProject_WhenProjectExists()
    {
        var projectDTO = new ProjectDTO
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Now
        };

        _service.CreateProject(projectDTO);

        var project = _service.GetProjectByName("Test Project");

        Assert.IsNotNull(project);
        Assert.AreEqual("Test Project", project.Name);
        Assert.AreEqual("Test Description", project.Description);
    }
}