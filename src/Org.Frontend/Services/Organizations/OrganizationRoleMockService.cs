using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Organizations;

public sealed class OrganizationRoleMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IOrganizationRoleService
{
    private static readonly HashSet<string> ProtectedRoleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "President",
        "VicePresident"
    };

    private static readonly IReadOnlyList<PermissionOptionViewModel> PermissionCatalog =
    [
        new() { Code = "org.overview.read", Group = "Overview", Label = "View organization overview" },
        new() { Code = "org.overview.write", Group = "Overview", Label = "Edit organization overview" },
        new() { Code = "org.workspace.access", Group = "Overview", Label = "Access organization workspace" },
        new() { Code = "org.members.manage", Group = "Members", Label = "Manage members" },
        new() { Code = "org.roles.view", Group = "Roles", Label = "View roles" },
        new() { Code = "org.roles.create", Group = "Roles", Label = "Create roles" },
        new() { Code = "org.roles.update", Group = "Roles", Label = "Update roles" },
        new() { Code = "org.roles.delete", Group = "Roles", Label = "Delete roles" },
        new() { Code = "org.roles.assign", Group = "Roles", Label = "Assign roles to members" },
        new() { Code = "org.events.manage", Group = "Events", Label = "Manage events" },
        new() { Code = "org.tasks.manage", Group = "Tasks", Label = "Manage tasks" },
        new() { Code = "org.requests.view", Group = "Requests", Label = "View organization requests" },
        new() { Code = "org.requests.review", Group = "Requests", Label = "Review organization requests" },
        new() { Code = "org.requests.approve", Group = "Requests", Label = "Approve organization requests" }
    ];

    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<bool> CanManageRolesAsync(Guid organizationId, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
            HasAnyPermission(data, currentUserId, organizationId, "org.roles.assign", "org.roles.update", "org.roles.create", "org.members.manage"), ct);
    }

    public Task<IReadOnlyList<PermissionOptionViewModel>> GetAvailablePermissionsAsync(Guid organizationId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<PermissionOptionViewModel>>(PermissionCatalog);

    public async Task<IReadOnlyList<OrganizationRoleViewModel>> GetRolesAsync(Guid organizationId, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            EnsureCanViewRoles(data, currentUserId, organizationId);
            return data.OrganizationRoles
                .Where(x => x.OrgId == organizationId)
                .OrderBy(x => ResolveRoleRank(x.RoleName))
                .ThenBy(x => x.RoleName, StringComparer.OrdinalIgnoreCase)
                .Select(x => MapRole(x, data))
                .ToList()
                .AsReadOnly();
        }, ct);
    }

    public async Task<OrganizationRoleViewModel> CreateRoleAsync(Guid organizationId, UpsertOrganizationRoleRequest request, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            EnsureCanManageRoles(data, currentUserId, organizationId, "org.roles.create");
            EnsureOrganizationExists(data, organizationId);

            var roleName = NormalizeRoleName(request.Name);
            if (ProtectedRoleNames.Contains(roleName))
            {
                throw new InvalidOperationException("Cannot create protected system role name.");
            }

            var exists = data.OrganizationRoles.Any(x =>
                x.OrgId == organizationId
                && string.Equals(x.RoleName, roleName, StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                throw new InvalidOperationException("Role name already exists in this organization.");
            }

            var role = new MockOrganizationRole
            {
                Id = Guid.NewGuid(),
                OrgId = organizationId,
                RoleName = roleName,
                Permissions = NormalizePermissionCodes(request.PermissionCodes)
            };
            data.OrganizationRoles.Add(role);

            return MapRole(role, data);
        }, ct);
    }

    public async Task<OrganizationRoleViewModel> UpdateRoleAsync(Guid roleId, UpsertOrganizationRoleRequest request, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        return await _mockDataStore.UseAsync(data =>
        {
            var role = data.OrganizationRoles.FirstOrDefault(x => x.Id == roleId)
                ?? throw new KeyNotFoundException($"Role {roleId} not found.");

            EnsureCanManageRoles(data, currentUserId, role.OrgId, "org.roles.update");

            if (ProtectedRoleNames.Contains(role.RoleName))
            {
                throw new InvalidOperationException("Protected system role cannot be edited.");
            }

            var updatedName = NormalizeRoleName(request.Name);
            var duplicate = data.OrganizationRoles.Any(x =>
                x.Id != role.Id
                && x.OrgId == role.OrgId
                && string.Equals(x.RoleName, updatedName, StringComparison.OrdinalIgnoreCase));
            if (duplicate)
            {
                throw new InvalidOperationException("Role name already exists in this organization.");
            }

            role.RoleName = updatedName;
            role.Permissions = NormalizePermissionCodes(request.PermissionCodes);
            return MapRole(role, data);
        }, ct);
    }

    public async Task DeleteRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        await _mockDataStore.UseAsync(data =>
        {
            var role = data.OrganizationRoles.FirstOrDefault(x => x.Id == roleId)
                ?? throw new KeyNotFoundException($"Role {roleId} not found.");

            EnsureCanManageRoles(data, currentUserId, role.OrgId, "org.roles.delete");

            if (ProtectedRoleNames.Contains(role.RoleName))
            {
                throw new InvalidOperationException("Protected system role cannot be deleted.");
            }

            var assignedCount = data.Members.Count(x => x.OrgId == role.OrgId && x.RoleId == role.Id);
            if (assignedCount > 0)
            {
                throw new InvalidOperationException("Cannot delete a role that is still assigned to members.");
            }

            data.OrganizationRoles.Remove(role);
            return 0;
        }, ct);
    }

    public async Task AssignRoleToMemberAsync(Guid organizationId, Guid memberId, Guid roleId, CancellationToken ct = default)
    {
        var currentUserId = await TryGetCurrentUserIdAsync();
        await _mockDataStore.UseAsync(data =>
        {
            EnsureCanManageRoles(data, currentUserId, organizationId, "org.roles.assign");

            var member = data.Members.FirstOrDefault(x => x.Id == memberId && x.OrgId == organizationId)
                ?? throw new KeyNotFoundException($"Member {memberId} not found in organization.");
            var role = data.OrganizationRoles.FirstOrDefault(x => x.Id == roleId && x.OrgId == organizationId)
                ?? throw new InvalidOperationException("Role does not belong to organization.");

            member.RoleId = role.Id;
            return 0;
        }, ct);
    }

    private static OrganizationRoleViewModel MapRole(MockOrganizationRole role, MockDataSet data)
    {
        return new OrganizationRoleViewModel
        {
            Id = role.Id,
            OrganizationId = role.OrgId,
            Name = role.RoleName,
            Description = null,
            PermissionCodes = role.Permissions
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            IsProtected = ProtectedRoleNames.Contains(role.RoleName),
            AssignedMemberCount = data.Members.Count(x => x.OrgId == role.OrgId && x.RoleId == role.Id)
        };
    }

    private static List<string> NormalizePermissionCodes(IReadOnlyList<string> permissions)
    {
        return permissions
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string NormalizeRoleName(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized is null || normalized.Length < 2)
        {
            throw new InvalidOperationException("Role name must be at least 2 characters.");
        }

        return normalized;
    }

    private static int ResolveRoleRank(string? roleName)
    {
        return roleName?.Trim().ToUpperInvariant() switch
        {
            "PRESIDENT" => 0,
            "VICEPRESIDENT" => 1,
            "MANAGER" => 2,
            "MEMBER" => 3,
            _ => 9
        };
    }

    private static void EnsureOrganizationExists(MockDataSet data, Guid organizationId)
    {
        if (!data.Organizations.Any(x => x.Id == organizationId))
        {
            throw new KeyNotFoundException($"Organization {organizationId} not found.");
        }
    }

    private static void EnsureCanViewRoles(MockDataSet data, Guid? userId, Guid organizationId)
    {
        if (!HasAnyPermission(data, userId, organizationId, "org.roles.view", "org.roles.assign", "org.members.manage", "org.workspace.access"))
        {
            throw new UnauthorizedAccessException("You do not have permission to view organization roles.");
        }
    }

    private static void EnsureCanManageRoles(MockDataSet data, Guid? userId, Guid organizationId, params string[] explicitPermissions)
    {
        if (HasAnyPermission(data, userId, organizationId, explicitPermissions))
        {
            return;
        }

        if (HasAnyPermission(data, userId, organizationId, "org.members.manage"))
        {
            return;
        }

        throw new UnauthorizedAccessException("You do not have permission to manage organization roles.");
    }

    private static bool HasAnyPermission(MockDataSet data, Guid? userId, Guid organizationId, params string[] expectedPermissions)
    {
        var member = ResolveMemberByUserId(data, organizationId, userId);
        if (member?.RoleId is null)
        {
            return false;
        }

        var role = data.OrganizationRoles.FirstOrDefault(x => x.Id == member.RoleId.Value && x.OrgId == organizationId);
        if (role is null)
        {
            return false;
        }

        var permissions = role.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return expectedPermissions.Any(permissions.Contains);
    }

    private static MockMember? ResolveMemberByUserId(MockDataSet data, Guid orgId, Guid? userId)
    {
        if (!userId.HasValue)
        {
            return null;
        }

        return data.Members.FirstOrDefault(x => x.OrgId == orgId && x.UserId == userId.Value);
    }

    private async Task<Guid?> TryGetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userIdText = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdText, out var userId) ? userId : null;
    }
}
