using Service.Models;

namespace Service.Interface;

public interface ILeaderPService
{
    void UpdateTask(string projectName, string taskTitle, TaskDTO taskDTO);

    TaskDTO GetTask(string projectName, string taskTitle);

    List<TaskDTO> GetTasks(string projectName);

    List<ProjectDTO> GetAllMyProjects();

    ProjectDTO GetProject(string projectName);

    void AssignMembersToProject(string projectName, List<UserDTO> membersDTO);

    List<UserDTO> GetAllMembersOfAProject(string projectName);

    void RemoveMemberFromProject(string projectName, string memberToRemoveEmail);

    List<TaskDTO> GetAllTaskForAMemberInAProject(string projectName, string memberEmail);

    void AddTaskToMember(string projectName, string memberEmail, string taskTitle);

    void RemoveTaskFromMember(string projectName, string memberEmail, string taskTitle);

    string ExportProjects();
}