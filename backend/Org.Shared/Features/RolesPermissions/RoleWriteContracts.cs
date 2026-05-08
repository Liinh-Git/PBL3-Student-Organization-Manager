namespace Org.Shared.Features.RolesPermissions;

// ============================================================
// REQUEST DTOs - Role Write Operations
// ============================================================

/// <summary>
/// Request DTO for creating role
/// </summary>
public record CreateRoleRequest
{
    /// <summary>
    /// Role name (required, 2-100 characters)
    /// </summary>
    public required string RoleName { get; init; }

    /// <summary>
    /// Description (optional)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Permission keys to assign to this role (optional)
    /// </summary>
    public List<string>? PermissionKeys { get; init; }
}

/// <summary>
/// Request DTO for updating role
/// </summary>
public record UpdateRoleRequest
{
    /// <summary>
    /// Role name (required, 2-100 characters)
    /// </summary>
    public required string RoleName { get; init; }

    /// <summary>
    /// Description (optional)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Permission keys to assign to this role (optional)
    /// </summary>
    public List<string>? PermissionKeys { get; init; }
}

/// <summary>
/// Request DTO for assigning role to member
/// </summary>
public record AssignRoleToMemberRequest
{
    /// <summary>
    /// Role ID to assign (required)
    /// </summary>
    public required Guid RoleId { get; init; }
}
