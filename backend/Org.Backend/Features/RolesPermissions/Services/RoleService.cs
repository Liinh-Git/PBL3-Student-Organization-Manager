using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.RolesPermissions.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Backend.Infrastructure.Persistence.Seed;
using Org.Shared.Features.RolesPermissions;

namespace Org.Backend.Features.RolesPermissions.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;

    public RoleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MyPermissionsResponse> GetMyPermissionsAsync(Guid orgId, Guid userId, CancellationToken ct = default)
    {
        // Find user's member record in this organization
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        if (member.Role == null)
        {
            throw new InvalidOperationException("Member does not have a role assigned");
        }

        var permissionKeys = member.Role.RolePermissions
            .Select(rp => rp.Permission.PermissionKey)
            .ToList();

        return new MyPermissionsResponse
        {
            PermissionKeys = permissionKeys,
            RoleId = member.RoleId ?? Guid.Empty,
            RoleName = member.Role.RoleName,
            MemberId = member.Id,
            OrganizationId = orgId
        };
    }

    public async Task<List<RoleDto>> GetOrganizationRolesAsync(Guid orgId, Guid userId, CancellationToken ct = default)
    {
        // Verify user is a member of this organization
        var isMember = await _context.Members
            .AnyAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You do not have access to this organization");
        }

        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Where(r => r.OrgId == orgId)
            .OrderBy(r => r.RoleName)
            .ToListAsync(ct);

        return roles.Select(r => r.ToRoleDto()).ToList();
    }

    public async Task<RoleDto> CreateRoleAsync(Guid orgId, Guid userId, CreateRoleRequest request, CancellationToken ct = default)
    {
        // Verify user is active member and has org.roles.create permission
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.roles.create");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to create roles");
        }

        // Check for duplicate role name in same organization
        var duplicateExists = await _context.Roles
            .AnyAsync(r => r.OrgId == orgId && r.RoleName == request.RoleName, ct);

        if (duplicateExists)
        {
            throw new InvalidOperationException($"Role with name '{request.RoleName}' already exists in this organization");
        }

        // Validate permission keys if provided
        List<Permission> permissions = new();
        if (request.PermissionKeys != null && request.PermissionKeys.Any())
        {
            var invalidKeys = request.PermissionKeys
                .Where(k => !SeedConstants.CanonicalPermissions.Contains(k))
                .ToList();

            if (invalidKeys.Any())
            {
                throw new InvalidOperationException($"Invalid permission keys: {string.Join(", ", invalidKeys)}");
            }

            permissions = await _context.Permissions
                .Where(p => request.PermissionKeys.Contains(p.PermissionKey))
                .ToListAsync(ct);
        }

        // Create role
        var role = new Role
        {
            OrgId = orgId,
            RoleName = request.RoleName,
            Description = request.Description,
            IsDefault = false,
            Level = null
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(ct);

        // Create role permissions
        if (permissions.Any())
        {
            var rolePermissions = permissions.Select(p => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = p.Id
            }).ToList();

            _context.RolePermissions.AddRange(rolePermissions);
            await _context.SaveChangesAsync(ct);
        }

        // Reload role with permissions
        var createdRole = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstAsync(r => r.Id == role.Id, ct);

        return createdRole.ToRoleDto();
    }

    public async Task<RoleDto> UpdateRoleAsync(Guid roleId, Guid userId, UpdateRoleRequest request, CancellationToken ct = default)
    {
        // Find role and resolve orgId
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId, ct);

        if (role == null)
        {
            throw new InvalidOperationException("Role not found");
        }

        var orgId = role.OrgId;

        // Verify user is active member and has org.roles.update permission
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.roles.update");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to update roles");
        }

        // Check for duplicate role name (excluding current role)
        var duplicateExists = await _context.Roles
            .AnyAsync(r => r.OrgId == orgId && r.RoleName == request.RoleName && r.Id != roleId, ct);

        if (duplicateExists)
        {
            throw new InvalidOperationException($"Role with name '{request.RoleName}' already exists in this organization");
        }

        // Update role fields
        role.RoleName = request.RoleName;
        role.Description = request.Description;

        // Update permissions if provided
        if (request.PermissionKeys != null)
        {
            // Validate permission keys
            var invalidKeys = request.PermissionKeys
                .Where(k => !SeedConstants.CanonicalPermissions.Contains(k))
                .ToList();

            if (invalidKeys.Any())
            {
                throw new InvalidOperationException($"Invalid permission keys: {string.Join(", ", invalidKeys)}");
            }

            // Remove existing role permissions
            _context.RolePermissions.RemoveRange(role.RolePermissions);

            // Add new role permissions
            if (request.PermissionKeys.Any())
            {
                var permissions = await _context.Permissions
                    .Where(p => request.PermissionKeys.Contains(p.PermissionKey))
                    .ToListAsync(ct);

                var rolePermissions = permissions.Select(p => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = p.Id
                }).ToList();

                _context.RolePermissions.AddRange(rolePermissions);
            }
        }

        await _context.SaveChangesAsync(ct);

        // Reload role with updated permissions
        var updatedRole = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstAsync(r => r.Id == roleId, ct);

        return updatedRole.ToRoleDto();
    }

    public async Task<bool> DeleteRoleAsync(Guid roleId, Guid userId, CancellationToken ct = default)
    {
        // Find role and resolve orgId
        var role = await _context.Roles
            .Include(r => r.Members)
            .FirstOrDefaultAsync(r => r.Id == roleId, ct);

        if (role == null)
        {
            throw new InvalidOperationException("Role not found");
        }

        var orgId = role.OrgId;

        // Verify user is active member and has org.roles.delete permission
        var member = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (member == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        if (member.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = member.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.roles.delete");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete roles");
        }

        // Prevent deletion of default roles
        if (role.IsDefault)
        {
            throw new InvalidOperationException("Cannot delete default role");
        }

        // Prevent deletion if active members are assigned
        var activeMembersCount = role.Members.Count(m => m.Status == MemberStatus.Active);
        if (activeMembersCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete role with {activeMembersCount} active member(s) assigned");
        }

        // Delete role (cascade will handle RolePermissions)
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> AssignRoleToMemberAsync(Guid orgId, Guid memberId, Guid userId, AssignRoleToMemberRequest request, CancellationToken ct = default)
    {
        // Verify user is active member and has org.roles.assign permission
        var currentMember = await _context.Members
            .Include(m => m.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(m => m.OrgId == orgId && m.UserId == userId && m.Status == MemberStatus.Active, ct);

        if (currentMember == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this organization");
        }

        if (currentMember.Role == null)
        {
            throw new UnauthorizedAccessException("You do not have a role assigned");
        }

        var hasPermission = currentMember.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionKey == "org.roles.assign");

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException("You do not have permission to assign roles");
        }

        // Find target member
        var targetMember = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId && m.OrgId == orgId, ct);

        if (targetMember == null)
        {
            throw new InvalidOperationException("Member not found in this organization");
        }

        // Verify role belongs to same organization
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId && r.OrgId == orgId, ct);

        if (role == null)
        {
            throw new InvalidOperationException("Role not found in this organization");
        }

        // Assign role
        targetMember.RoleId = request.RoleId;
        await _context.SaveChangesAsync(ct);

        return true;
    }
}
