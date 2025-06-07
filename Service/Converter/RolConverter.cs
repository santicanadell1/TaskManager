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
                _ => throw new ArgumentException("RolDTO desconocido")
            }).ToList();
    }
}