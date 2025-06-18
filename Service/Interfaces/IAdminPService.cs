using Service.Models;

namespace Service.Interface;

public interface IAdminPService
{
    void CreateProject(ProjectDTO projectDTO);
    void UpdateProject(string projectNameToUpdate, ProjectDTO updatedProjectDTO);
    void RemoveProject(string projectName);
    List<ProjectDTO> GetAllProjects();
    ProjectDTO GetProjectByName(string projectName);
    List<ProjectDTO> GetAllProjectsForUser(string email);


    void AssignMembersToProject(string projectName, List<UserDTO> membersDTO);
    void RemoveMemberFromProject(string projectName, string memberEmail);
    List<UserDTO> GetMembers(string projectName);
    void AddTaskToMember(string projectName, string memberEmail, string taskID);
    void RemoveTaskFromMember(string projectName, string memberEmail, string taskID);


    List<TaskDTO> GetAllTaskForAMember(string email);
    List<TaskDTO> GetAllTaskForAMemberInAProject(string projectName, string email);

    void SetProjectLeader(string projectName, string LeaderEmail);

    List<UserDTO> GetAllProjectLeaderUsers();

    void RemoveProjectLeader(string projectName);
    public List<ProjectDTO> GetAllProjectsForAdmin();
    void RemoveAdminFromProject(string projectName, string userDtoEmail);
}