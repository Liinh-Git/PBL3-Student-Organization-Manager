namespace Org.Shared.Features.Discover;

// ============================================================
// RESPONSE DTOs
// ============================================================

/// <summary>
/// Discover event DTO for public event discovery
/// </summary>
public record DiscoverEventDto
{
    public required Guid Id { get; init; }
    public required Guid OrganizationId { get; init; }
    public required string OrganizationName { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public string? Location { get; init; }
    public required string Visibility { get; init; } // Public, OrganizationOnly, Private
    public required string Status { get; init; } // Draft, Published, Ongoing, Completed, Cancelled, Archived
}
