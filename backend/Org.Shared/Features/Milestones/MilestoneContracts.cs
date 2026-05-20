namespace Org.Shared.Features.Milestones;

/// <summary>
/// Request DTO for creating a milestone
/// </summary>
public record CreateMilestoneRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? OrderIndex { get; init; }
}

/// <summary>
/// Request DTO for updating a milestone
/// </summary>
public record UpdateMilestoneRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public required string Status { get; init; } // Planned, InProgress, Completed, Archived
    public int? OrderIndex { get; init; }
}

/// <summary>
/// Response DTO for milestone
/// </summary>
public record MilestoneDto
{
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public required string Status { get; init; }
    public required int OrderIndex { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
