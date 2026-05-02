using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Features.Common;
using Org.Backend.Infrastructure.Database;
using Org.Shared.Features.Organizations;

namespace Org.Backend.Features.Organizations;

public sealed class GetPublicOrganizationOverviewEndpoint(AppDbContext db)
    : EndpointWithoutRequest<GetPublicOrganizationOverviewResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/{id:guid}/public-overview");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var organization = await db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (organization is null)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var dto = new PublicOrganizationOverviewDto(
            organization!.Id,
            organization.OrgName,
            organization.Description,
            organization.AvatarUrl,
            organization.CoverUrl,
            organization.Location,
            organization.FoundingDate is null ? null : DateOnly.FromDateTime(organization.FoundingDate.Value),
            organization.TotalMembers,
            !organization.IsDeleted && organization.Status == Domain.Enums.OrgStatus.Active,
            OrganizationRoleEndpointMapping.ToUtcOffset(organization.CreatedAt),
            organization.UpdatedAt is null ? null : OrganizationRoleEndpointMapping.ToUtcOffset(organization.UpdatedAt.Value));

        await Send.OkAsync(new GetPublicOrganizationOverviewResponse(dto), ct);
    }
}

public sealed class GetOrganizationPermissionsMeEndpoint(AppDbContext db)
    : EndpointWithoutRequest<GetOrganizationPermissionsMeResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/{id:guid}/permissions/me");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgId = Route<Guid>("id");
        var organizationExists = await db.Organizations.AnyAsync(x => x.Id == orgId, ct);
        if (!organizationExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var caller = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, orgId, ct);
        if (caller is null)
        {
            var guest = new OrganizationPermissionDto(false, false, false, false, false, false, false, false, false, false, null, []);
            await Send.OkAsync(new GetOrganizationPermissionsMeResponse(guest), ct);
            return;
        }

        var permissionKeys = await OrganizationPermissionCatalog.GetRolePermissionKeysAsync(db, caller.Value.RoleId, ct);
        var dto = OrganizationPermissionCatalog.BuildPermissionDto(
            isAuthenticated: true,
            isMember: true,
            memberRole: caller.Value.Role.ToString(),
            permissionKeys);

        await Send.OkAsync(new GetOrganizationPermissionsMeResponse(dto), ct);
    }
}

public sealed class GetOrganizationPermissionsCatalogEndpoint(AppDbContext db)
    : EndpointWithoutRequest<GetOrganizationPermissionsCatalogResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/{id:guid}/permissions");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgId = Route<Guid>("id");
        var organizationExists = await db.Organizations.AnyAsync(x => x.Id == orgId, ct);
        if (!organizationExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var caller = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, orgId, ct);
        if (caller is null)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var permissionKeys = await OrganizationPermissionCatalog.GetRolePermissionKeysAsync(db, caller.Value.RoleId, ct);
        var capabilities = OrganizationPermissionCatalog.BuildPermissionDto(true, true, caller.Value.Role.ToString(), permissionKeys);
        if (!capabilities.CanManageRoles)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        await OrganizationPermissionCatalog.EnsureCatalogExistsAsync(db, ct);

        var items = await db.Permissions
            .AsNoTracking()
            .Where(x => x.PermissionKey.StartsWith("org."))
            .OrderBy(x => x.ModuleGroup)
            .ThenBy(x => x.PermissionKey)
            .Select(x => new PermissionCatalogItemDto(
                x.PermissionKey,
                string.IsNullOrWhiteSpace(x.DisplayName) ? x.PermissionKey : x.DisplayName,
                string.IsNullOrWhiteSpace(x.ModuleGroup) ? "General" : x.ModuleGroup!))
            .ToListAsync(ct);

        await Send.OkAsync(new GetOrganizationPermissionsCatalogResponse(items), ct);
    }
}

public sealed class GetOrganizationRolesEndpoint(AppDbContext db)
    : EndpointWithoutRequest<GetOrganizationRolesResponse>
{
    public override void Configure()
    {
        Get("/api/organizations/{id:guid}/roles");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgId = Route<Guid>("id");
        var organizationExists = await db.Organizations.AnyAsync(x => x.Id == orgId, ct);
        if (!organizationExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var caller = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, orgId, ct);
        if (caller is null)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var permissionKeys = await OrganizationPermissionCatalog.GetRolePermissionKeysAsync(db, caller.Value.RoleId, ct);
        var capabilities = OrganizationPermissionCatalog.BuildPermissionDto(true, true, caller.Value.Role.ToString(), permissionKeys);
        if (!capabilities.CanManageRoles)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var roles = await db.Roles
            .AsNoTracking()
            .Where(x => x.OrgId == orgId)
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .OrderBy(x => x.RoleName)
            .ToListAsync(ct);

        var roleIds = roles.Select(x => x.Id).ToList();
        var assignedMap = await db.Members
            .AsNoTracking()
            .Where(x => x.OrgId == orgId && x.RoleId.HasValue && roleIds.Contains(x.RoleId.Value))
            .GroupBy(x => x.RoleId!.Value)
            .Select(x => new { RoleId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.RoleId, x => x.Count, ct);

        var items = roles.Select(role => new OrganizationRoleDto(
            role.Id,
            role.OrgId,
            role.RoleName,
            role.Description,
            OrganizationPermissionCatalog.IsProtectedRole(role.RoleName, role.IsDefault),
            assignedMap.GetValueOrDefault(role.Id),
            role.RolePermissions
                .Select(x => x.Permission.PermissionKey)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList()))
            .ToList();

        await Send.OkAsync(new GetOrganizationRolesResponse(items), ct);
    }
}

public sealed class CreateOrganizationRoleEndpoint(AppDbContext db)
    : Endpoint<UpsertOrganizationRoleRequest, OrganizationRoleDto>
{
    public override void Configure()
    {
        Post("/api/organizations/{id:guid}/roles");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpsertOrganizationRoleRequest req, CancellationToken ct)
    {
        var orgId = Route<Guid>("id");
        var organizationExists = await db.Organizations.AnyAsync(x => x.Id == orgId, ct);
        if (!organizationExists)
            ThrowError("Organization not found.", StatusCodes.Status404NotFound);

        var caller = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, orgId, ct);
        if (caller is null)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var callerPermissions = await OrganizationPermissionCatalog.GetRolePermissionKeysAsync(db, caller.Value.RoleId, ct);
        var capabilities = OrganizationPermissionCatalog.BuildPermissionDto(true, true, caller.Value.Role.ToString(), callerPermissions);
        if (!capabilities.CanManageRoles)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        await OrganizationPermissionCatalog.EnsureCatalogExistsAsync(db, ct);

        var roleName = OrganizationRoleEndpointMapping.NormalizeRoleName(req.Name);
        var duplicateName = await db.Roles
            .AnyAsync(x => x.OrgId == orgId && x.RoleName.ToLower() == roleName.ToLower(), ct);
        if (duplicateName)
            ThrowError("Role name already exists in this organization.", StatusCodes.Status409Conflict);

        if (OrganizationPermissionCatalog.IsProtectedRole(roleName, isDefault: false))
            ThrowError("Cannot create role with protected system role name.", StatusCodes.Status400BadRequest);

        var permissionKeys = OrganizationRoleEndpointMapping.NormalizePermissionCodes(req.PermissionCodes);
        var permissionIds = await OrganizationRoleEndpointMapping.ResolvePermissionIdsAsync(db, permissionKeys, ct);

        var role = new Domain.Entities.Role
        {
            OrgId = orgId,
            RoleName = roleName,
            Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim(),
            IsDefault = false
        };

        db.Roles.Add(role);
        await db.SaveChangesAsync(ct);

        if (permissionIds.Count > 0)
        {
            foreach (var permissionId in permissionIds)
            {
                db.RolePermissions.Add(new Domain.Entities.RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                });
            }

            await db.SaveChangesAsync(ct);
        }

        var dto = new OrganizationRoleDto(
            role.Id,
            role.OrgId,
            role.RoleName,
            role.Description,
            false,
            0,
            permissionKeys);

        await HttpContext.Response.SendAsync(dto, StatusCodes.Status201Created, cancellation: ct);
    }
}

public sealed class UpdateOrganizationRoleEndpoint(AppDbContext db)
    : Endpoint<UpsertOrganizationRoleRequest, OrganizationRoleDto>
{
    public override void Configure()
    {
        Put("/api/organizations/roles/{roleId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpsertOrganizationRoleRequest req, CancellationToken ct)
    {
        var roleId = Route<Guid>("roleId");
        var role = await db.Roles
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(x => x.Id == roleId, ct);

        if (role is null)
            ThrowError("Role not found.", StatusCodes.Status404NotFound);

        var caller = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, role!.OrgId, ct);
        if (caller is null)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var callerPermissions = await OrganizationPermissionCatalog.GetRolePermissionKeysAsync(db, caller.Value.RoleId, ct);
        var capabilities = OrganizationPermissionCatalog.BuildPermissionDto(true, true, caller.Value.Role.ToString(), callerPermissions);
        if (!capabilities.CanManageRoles)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        if (OrganizationPermissionCatalog.IsProtectedRole(role.RoleName, role.IsDefault))
            ThrowError("Protected role cannot be updated.", StatusCodes.Status400BadRequest);

        var newRoleName = OrganizationRoleEndpointMapping.NormalizeRoleName(req.Name);
        var duplicateName = await db.Roles
            .AnyAsync(x => x.Id != role.Id && x.OrgId == role.OrgId && x.RoleName.ToLower() == newRoleName.ToLower(), ct);
        if (duplicateName)
            ThrowError("Role name already exists in this organization.", StatusCodes.Status409Conflict);

        if (OrganizationPermissionCatalog.IsProtectedRole(newRoleName, isDefault: false))
            ThrowError("Cannot rename to protected system role name.", StatusCodes.Status400BadRequest);

        await OrganizationPermissionCatalog.EnsureCatalogExistsAsync(db, ct);
        var permissionKeys = OrganizationRoleEndpointMapping.NormalizePermissionCodes(req.PermissionCodes);
        var permissionIds = await OrganizationRoleEndpointMapping.ResolvePermissionIdsAsync(db, permissionKeys, ct);

        role.RoleName = newRoleName;
        role.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();

        db.RolePermissions.RemoveRange(role.RolePermissions);
        foreach (var permissionId in permissionIds)
        {
            db.RolePermissions.Add(new Domain.Entities.RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId
            });
        }

        await db.SaveChangesAsync(ct);

        var assignedMemberCount = await db.Members
            .AsNoTracking()
            .CountAsync(x => x.RoleId == role.Id, ct);

        var dto = new OrganizationRoleDto(
            role.Id,
            role.OrgId,
            role.RoleName,
            role.Description,
            false,
            assignedMemberCount,
            permissionKeys);

        await Send.OkAsync(dto, ct);
    }
}

public sealed class DeleteOrganizationRoleEndpoint(AppDbContext db)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/organizations/roles/{roleId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var roleId = Route<Guid>("roleId");
        var role = await db.Roles
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(x => x.Id == roleId, ct);

        if (role is null)
            ThrowError("Role not found.", StatusCodes.Status404NotFound);

        var caller = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, role!.OrgId, ct);
        if (caller is null)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var callerPermissions = await OrganizationPermissionCatalog.GetRolePermissionKeysAsync(db, caller.Value.RoleId, ct);
        var capabilities = OrganizationPermissionCatalog.BuildPermissionDto(true, true, caller.Value.Role.ToString(), callerPermissions);
        if (!capabilities.CanManageRoles)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        if (OrganizationPermissionCatalog.IsProtectedRole(role.RoleName, role.IsDefault))
            ThrowError("Protected role cannot be deleted.", StatusCodes.Status400BadRequest);

        var assigned = await db.Members.AnyAsync(x => x.RoleId == role.Id, ct);
        if (assigned)
            ThrowError("Role is assigned to one or more members and cannot be deleted.", StatusCodes.Status409Conflict);

        db.RolePermissions.RemoveRange(role.RolePermissions);
        db.Roles.Remove(role);
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

public sealed class AssignRoleToOrganizationMemberEndpoint(AppDbContext db)
    : Endpoint<AssignOrganizationRoleRequest, Org.Shared.Features.Members.MemberDto>
{
    public override void Configure()
    {
        Post("/api/organizations/{id:guid}/members/{memberId:guid}/role");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(AssignOrganizationRoleRequest req, CancellationToken ct)
    {
        var orgId = Route<Guid>("id");
        var memberId = Route<Guid>("memberId");

        var caller = await OrganizationAuthorization.ResolveCallerContextAsync(db, User, orgId, ct);
        if (caller is null)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var callerPermissions = await OrganizationPermissionCatalog.GetRolePermissionKeysAsync(db, caller.Value.RoleId, ct);
        var capabilities = OrganizationPermissionCatalog.BuildPermissionDto(true, true, caller.Value.Role.ToString(), callerPermissions);
        if (!capabilities.CanManageRoles)
            ThrowError("Forbidden.", StatusCodes.Status403Forbidden);

        var member = await db.Members
            .Include(x => x.User)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == memberId && x.OrgId == orgId, ct);

        if (member is null)
            ThrowError("Member not found in organization.", StatusCodes.Status404NotFound);

        var targetRole = await db.Roles.FirstOrDefaultAsync(x => x.Id == req.RoleId && x.OrgId == orgId, ct);
        if (targetRole is null)
            ThrowError("Role does not belong to organization.", StatusCodes.Status400BadRequest);

        if (OrganizationPermissionCatalog.IsProtectedRole(targetRole.RoleName, targetRole.IsDefault)
            && caller.Value.Role < Org.Shared.MemberRole.VicePresident)
        {
            ThrowError("Only high-level organization managers can assign protected roles.", StatusCodes.Status403Forbidden);
        }

        member.RoleId = targetRole.Id;
        member.Role = targetRole;
        await db.SaveChangesAsync(ct);

        await Send.OkAsync(ContractMapping.ToMemberDto(member), ct);
    }
}

internal static class OrganizationRoleEndpointMapping
{
    public static List<string> NormalizePermissionCodes(IReadOnlyList<string>? permissionCodes)
    {
        if (permissionCodes is null)
            return [];

        return permissionCodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string NormalizeRoleName(string? roleName)
    {
        var normalized = string.IsNullOrWhiteSpace(roleName) ? null : roleName.Trim();
        if (normalized is null || normalized.Length < 2)
            throw new InvalidOperationException("Role name must be at least 2 characters.");

        return normalized;
    }

    public static async Task<List<Guid>> ResolvePermissionIdsAsync(AppDbContext db, IReadOnlyList<string> permissionKeys, CancellationToken ct)
    {
        if (permissionKeys.Count == 0)
            return [];

        var permissions = await db.Permissions
            .AsNoTracking()
            .Where(x => permissionKeys.Contains(x.PermissionKey))
            .Select(x => new { x.Id, x.PermissionKey })
            .ToListAsync(ct);

        var map = permissions.ToDictionary(x => x.PermissionKey, x => x.Id, StringComparer.OrdinalIgnoreCase);
        var missing = permissionKeys.Where(x => !map.ContainsKey(x)).ToList();
        if (missing.Count > 0)
            throw new InvalidOperationException($"Permission codes not found: {string.Join(", ", missing)}");

        return permissionKeys.Select(x => map[x]).ToList();
    }

    public static DateTimeOffset ToUtcOffset(DateTime value)
        => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
