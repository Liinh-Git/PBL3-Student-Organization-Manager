namespace Org.Shared.Features.RolesPermissions;

// ============================================================
// RESPONSE DTOs
// ============================================================

/// <summary>
/// Permission DTO
/// </summary>
public record PermissionDto
{
    public required Guid Id { get; init; }
    public required string PermissionKey { get; init; }
    public required string DisplayName { get; init; }
    public required string ModuleGroup { get; init; }
}

/// <summary>
/// My permissions response for current user in organization
/// </summary>
public record MyPermissionsResponse
{
    public required List<string> PermissionKeys { get; init; }
    public required Guid RoleId { get; init; }
    public required string RoleName { get; init; }
    public required Guid MemberId { get; init; }
    public required Guid OrganizationId { get; init; }
}

/// <summary>
/// Role DTO
/// </summary>
public record RoleDto
{
    public required Guid Id { get; init; }
    public required Guid OrganizationId { get; init; }
    public required string RoleName { get; init; }
    public string? Description { get; init; }
    public required bool IsDefault { get; init; }
    public required List<string> PermissionKeys { get; init; }
}
