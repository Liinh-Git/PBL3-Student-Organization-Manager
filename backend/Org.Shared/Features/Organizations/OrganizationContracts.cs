namespace Org.Shared.Features.Organizations;

// ============================================================
// RESPONSE DTOs
// ============================================================

/// <summary>
/// Full organization DTO
/// </summary>
public record OrganizationDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CoverUrl { get; init; }
    public DateTime? FoundingDate { get; init; }
    public string? Location { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public required int TotalMembers { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
}

/// <summary>
/// Organization summary DTO for list views
/// </summary>
public record OrganizationSummaryDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? AvatarUrl { get; init; }
    public required int TotalMembers { get; init; }
    public required string Status { get; init; }
}

/// <summary>
/// Organization public overview DTO (no auth required)
/// </summary>
public record OrganizationPublicOverviewDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CoverUrl { get; init; }
    public required int TotalMembers { get; init; }
    public int? PublicEventsCount { get; init; }
    public int? DepartmentsCount { get; init; }
}
