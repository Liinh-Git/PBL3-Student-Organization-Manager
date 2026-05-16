namespace Org.Shared.Features.Events;

/// <summary>
/// Full event DTO for detail views
/// </summary>
public record EventDto
{
    public required Guid Id { get; init; }
    public required Guid OrganizationId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public required string Status { get; init; }
    public required string Visibility { get; init; }
    public string? Location { get; init; }
    public string? BannerUrl { get; init; }
    public int? TargetParticipants { get; init; }
    public decimal? Budget { get; init; }
    public double? AverageRating { get; init; }
    public string? Tags { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
}

/// <summary>
/// Event summary DTO for list views
/// </summary>
public record EventSummaryDto
{
    public required Guid Id { get; init; }
    public required Guid OrganizationId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public required string Status { get; init; }
    public required string Visibility { get; init; }
    public string? Location { get; init; }
    public string? BannerUrl { get; init; }
    public int? TargetParticipants { get; init; }
}

/// <summary>
/// Public event DTO for public discovery (no auth required)
/// </summary>
public record EventPublicDto
{
    public required Guid Id { get; init; }
    public required Guid OrganizationId { get; init; }
    public required string OrganizationName { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public string? Location { get; init; }
    public string? BannerUrl { get; init; }
    public required string Visibility { get; init; }
    public required string Status { get; init; }
}
