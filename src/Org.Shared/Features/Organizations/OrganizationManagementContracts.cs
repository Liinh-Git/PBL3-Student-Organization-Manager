namespace Org.Shared.Features.Organizations;

public sealed record PublicOrganizationOverviewDto(
    Guid Id,
    string Name,
    string? Description,
    string? AvatarUrl,
    string? CoverUrl,
    string? Location,
    DateOnly? FoundingDate,
    int TotalMembers,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

public sealed record GetPublicOrganizationOverviewResponse(PublicOrganizationOverviewDto Data);

public sealed record OrganizationPermissionDto(
    bool IsAuthenticated,
    bool IsMember,
    bool CanAccessWorkspace,
    bool CanEditOverview,
    bool CanManageMembers,
    bool CanCreateEvents,
    bool CanViewRequests,
    bool CanReviewRequests,
    bool CanManageRoles,
    bool CanManageDepartments,
    string? MemberRole,
    IReadOnlyList<string> PermissionCodes);

public sealed record GetOrganizationPermissionsMeResponse(OrganizationPermissionDto Data);

public sealed record PermissionCatalogItemDto(
    string Code,
    string Label,
    string Group);

public sealed record GetOrganizationPermissionsCatalogResponse(IReadOnlyList<PermissionCatalogItemDto> Items);

public sealed record OrganizationRoleDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    bool IsProtected,
    int AssignedMemberCount,
    IReadOnlyList<string> PermissionCodes);

public sealed record GetOrganizationRolesResponse(IReadOnlyList<OrganizationRoleDto> Items);

public sealed record UpsertOrganizationRoleRequest(
    string Name,
    string? Description,
    IReadOnlyList<string> PermissionCodes);

public sealed record AssignOrganizationRoleRequest(Guid RoleId);
