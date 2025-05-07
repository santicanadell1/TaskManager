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
}