using Service.Models;
using System;
using System.Collections.Generic;

public interface IAdminPService
{
    // Project management
    void CreateProject(ProjectDTO projectDTO);
    void UpdateProject(string projectNameToUpdate, ProjectDTO updatedProjectDTO);
    void RemoveProject(string projectName);
    List<ProjectDTO> GetAllProjects();
    ProjectDTO GetProjectByName(string projectName);
    List<ProjectDTO> GetAllProjectsFromProject(string email);

    // Member management
    void AssignMembersToProject(string projectName, List<UserDTO> membersDTO);
    void RemoveMemberFromProject(string projectName, string memberEmail);
    List<UserDTO> GetMembers(string projectName);
    void AddTaskToMember(string projectName, string memberEmail, int taskID);
    void RemoveTaskFromMember(string projectName, string memberEmail, int taskID);

    // Task management for members
    List<TaskDTO> GetAllTaskForAMember(string email);
    List<TaskDTO> GetAllTaskForAMemberInAProject(string projectName, string email);
}