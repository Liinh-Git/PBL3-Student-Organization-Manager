namespace Org.Shared.Features.Users;

// ============================================================
// RESPONSE DTOs
// ============================================================

/// <summary>
/// User profile DTO
/// </summary>
public record UserProfileDto
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Bio { get; init; }
    public required string Status { get; init; }
    public string? ProfileVisibility { get; init; }
    public DateTime? LastLoginAtUtc { get; init; }
}

/// <summary>
/// My organization DTO for user's organization list
/// </summary>
public record MyOrganizationDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CoverUrl { get; init; }
    public DateTime? FoundingDate { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required Guid RoleId { get; init; }
    public required string RoleName { get; init; }
    public required Guid MemberId { get; init; }
    public required DateTime JoinedAtUtc { get; init; }
    public bool? IsDefault { get; init; }
}

/// <summary>
/// My event DTO for user's event list
/// </summary>
public record MyEventDto
{
    public required Guid Id { get; init; }
    public required Guid OrganizationId { get; init; }
    public required string OrganizationName { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public string? BannerUrl { get; init; }
    public required string Status { get; init; }
    public required string Visibility { get; init; }
    public string? Location { get; init; }
}

/// <summary>
/// Discover organization DTO for public organization discovery
/// </summary>
public record DiscoverOrganizationDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CoverUrl { get; init; }
    public required int TotalMembers { get; init; }
    public required string Status { get; init; }
}
