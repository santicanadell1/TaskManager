using Domain;
using Service.Models;

namespace Service.Converter;

public class RolConverter
{
    public List<RolDTO> ConvertToDTORoles(List<Rol> roles)
    {
        return roles.Select(role =>
            role switch
            {
                Rol.AdminSystem => RolDTO.AdminSystem,
                Rol.ProjectMember => RolDTO.ProjectMember,
                Rol.AdminProject => RolDTO.AdminProject,
                Rol.ProjectLeader => RolDTO.ProjectLeader,
                _ => throw new ArgumentException("Rol desconocido")
            }).ToList();
    }

    public List<Rol> ConvertToDomainRoles(List<RolDTO> roleDTOs)
    {
        return roleDTOs.Select(roleDTO =>
            roleDTO switch
            {
                RolDTO.AdminSystem => Rol.AdminSystem,
                RolDTO.ProjectMember => Rol.ProjectMember,
                RolDTO.AdminProject => Rol.AdminProject,
                RolDTO.ProjectLeader => Rol.ProjectLeader,
                _ => throw new ArgumentException("RolDTO desconocido")
            }).ToList();
    }

    public RolDTO ConvertToDTORole(Rol role)
    {
        switch (role)
        {
            case Rol.AdminSystem:
                return RolDTO.AdminSystem;
            case Rol.ProjectMember:
                return RolDTO.ProjectMember;
            case Rol.AdminProject:
                return RolDTO.AdminProject;
            case Rol.ProjectLeader:
                return RolDTO.ProjectLeader;
            default:
                throw new ArgumentException("Invalid role");
        }
    }
}