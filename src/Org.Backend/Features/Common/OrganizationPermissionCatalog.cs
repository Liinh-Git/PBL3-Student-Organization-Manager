using Microsoft.EntityFrameworkCore;
using Org.Backend.Infrastructure.Database;
using Org.Shared.Features.Organizations;

namespace Org.Backend.Features.Common;

internal static class OrganizationPermissionCatalog
{
    private static readonly (string Code, string Label, string Group)[] Catalog =
    [
        ("org.overview.read", "View organization overview", "Overview"),
        ("org.overview.write", "Edit organization overview", "Overview"),
        ("org.workspace.access", "Access organization workspace", "Overview"),
        ("org.members.manage", "Manage organization members", "Members"),
        ("org.roles.view", "View organization roles", "Roles"),
        ("org.roles.create", "Create organization roles", "Roles"),
        ("org.roles.update", "Update organization roles", "Roles"),
        ("org.roles.delete", "Delete organization roles", "Roles"),
        ("org.roles.assign", "Assign role to member", "Roles"),
        ("org.events.create", "Create organization events", "Events"),
        ("org.events.manage", "Manage organization events", "Events"),
        ("org.departments.manage", "Manage organization departments", "Departments"),
        ("org.requests.view", "View organization requests", "Requests"),
        ("org.requests.review", "Review organization requests", "Requests"),
        ("org.requests.approve", "Approve organization requests", "Requests")
    ];

    public static IReadOnlyList<PermissionCatalogItemDto> GetDefaultPermissionCatalog()
        => Catalog.Select(x => new PermissionCatalogItemDto(x.Code, x.Label, x.Group)).ToList();

    public static bool IsProtectedRole(string roleName, bool isDefault)
    {
        if (isDefault)
            return true;

        return roleName.Trim().ToUpperInvariant() switch
        {
            "PRESIDENT" => true,
            "VICEPRESIDENT" => true,
            "OWNER" => true,
            "FOUNDER" => true,
            _ => false
        };
    }

    public static async Task EnsureCatalogExistsAsync(AppDbContext db, CancellationToken ct)
    {
        var existing = await db.Permissions
            .AsNoTracking()
            .Where(x => Catalog.Select(c => c.Code).Contains(x.PermissionKey))
            .Select(x => x.PermissionKey)
            .ToListAsync(ct);

        var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var item in Catalog.Where(x => !existingSet.Contains(x.Code)))
        {
            db.Permissions.Add(new Domain.Entities.Permission
            {
                PermissionKey = item.Code,
                DisplayName = item.Label,
                ModuleGroup = item.Group
            });
        }

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync(ct);
    }

    public static async Task<HashSet<string>> GetRolePermissionKeysAsync(AppDbContext db, Guid? roleId, CancellationToken ct)
    {
        if (!roleId.HasValue)
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var keys = await db.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == roleId.Value)
            .Select(x => x.Permission.PermissionKey)
            .ToListAsync(ct);

        return keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public static OrganizationPermissionDto BuildPermissionDto(bool isAuthenticated, bool isMember, string? memberRole, HashSet<string> permissionKeys)
    {
        if (!isAuthenticated || !isMember)
        {
            return new OrganizationPermissionDto(
                isAuthenticated,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                memberRole,
                []);
        }

        var parsedRole = OrganizationAuthorization.ParseRoleName(memberRole);
        var fallbackCanRead = OrganizationAuthorization.CanRead(parsedRole);
        var fallbackCanPlan = OrganizationAuthorization.CanPlan(parsedRole);
        var fallbackCanDelete = OrganizationAuthorization.CanDelete(parsedRole);
        var hasFineGrainedPermissions = permissionKeys.Any(x => x.StartsWith("org.", StringComparison.OrdinalIgnoreCase));

        bool Resolve(bool fallback, params string[] keys)
        {
            if (!hasFineGrainedPermissions)
                return fallback;

            return keys.Any(permissionKeys.Contains);
        }

        var canAccessWorkspace = Resolve(fallbackCanRead, "org.workspace.access", "org.overview.read");
        var canEditOverview = Resolve(fallbackCanPlan, "org.overview.write");
        var canManageMembers = Resolve(fallbackCanPlan, "org.members.manage");
        var canCreateEvents = Resolve(fallbackCanPlan, "org.events.create", "org.events.manage");
        var canViewRequests = Resolve(fallbackCanPlan, "org.requests.view", "org.requests.review", "org.requests.approve");
        var canReviewRequests = Resolve(fallbackCanDelete, "org.requests.review", "org.requests.approve");
        var canManageRoles = Resolve(fallbackCanDelete, "org.roles.view", "org.roles.create", "org.roles.update", "org.roles.delete", "org.roles.assign");
        var canManageDepartments = Resolve(fallbackCanPlan, "org.departments.manage", "org.members.manage");

        return new OrganizationPermissionDto(
            true,
            true,
            canAccessWorkspace,
            canEditOverview,
            canManageMembers,
            canCreateEvents,
            canViewRequests,
            canReviewRequests,
            canManageRoles,
            canManageDepartments,
            memberRole,
            permissionKeys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList());
    }
}
