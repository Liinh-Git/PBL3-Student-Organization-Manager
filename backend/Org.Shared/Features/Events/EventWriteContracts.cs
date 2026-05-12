namespace Org.Shared.Features.Events;

// ============================================================
// REQUEST DTOs - Event Write Operations
// ============================================================

/// <summary>
/// Request DTO for creating event
/// </summary>
public record CreateEventRequest
{
    /// <summary>
    /// Event name (required, 2-200 characters)
    /// </summary>
    public required string EventName { get; init; }

    /// <summary>
    /// Description (optional)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Start date (required)
    /// </summary>
    public required DateTime StartDate { get; init; }

    /// <summary>
    /// End date (optional)
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Location (optional)
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Banner URL (optional)
    /// </summary>
    public string? BannerUrl { get; init; }

    /// <summary>
    /// Visibility (optional, defaults to "Internal")
    /// Valid values: "Public", "Internal", "Private"
    /// </summary>
    public string? Visibility { get; init; }

    /// <summary>
    /// Target number of participants (optional)
    /// </summary>
    public int? TargetParticipants { get; init; }
}

/// <summary>
/// Request DTO for updating event
/// </summary>
public record UpdateEventRequest
{
    /// <summary>
    /// Event name (required, 2-200 characters)
    /// </summary>
    public required string EventName { get; init; }

    /// <summary>
    /// Description (optional)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Start date (required)
    /// </summary>
    public required DateTime StartDate { get; init; }

    /// <summary>
    /// End date (optional)
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Location (optional)
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Banner URL (optional)
    /// </summary>
    public string? BannerUrl { get; init; }

    /// <summary>
    /// Visibility (optional, defaults to "Internal")
    /// Valid values: "Public", "Internal", "Private"
    /// </summary>
    public string? Visibility { get; init; }

    /// <summary>
    /// Target number of participants (optional)
    /// </summary>
    public int? TargetParticipants { get; init; }
}
