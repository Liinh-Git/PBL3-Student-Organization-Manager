using Org.Backend.Domain.Entities;
using Org.Shared.Features.RolesPermissions;

namespace Org.Backend.Features.RolesPermissions.Mappings;

public static class RoleMappings
{
    public static PermissionDto ToPermissionDto(this Permission permission)
    {
        return new PermissionDto
        {
            Id = permission.Id,
            PermissionKey = permission.PermissionKey,
            DisplayName = permission.DisplayName,
            ModuleGroup = permission.ModuleGroup
        };
    }

    public static RoleDto ToRoleDto(this Role role)
    {
        var permissionKeys = role.RolePermissions
            .Select(rp => rp.Permission.PermissionKey)
            .ToList();

        return new RoleDto
        {
            Id = role.Id,
            OrganizationId = role.OrgId,
            RoleName = role.RoleName,
            Description = role.Description,
            IsDefault = role.IsDefault,
            PermissionKeys = permissionKeys
        };
    }
}
