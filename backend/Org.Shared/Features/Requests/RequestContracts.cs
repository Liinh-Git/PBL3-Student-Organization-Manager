namespace Org.Shared.Features.Requests;

// ============================================================
// REQUEST DTOs
// ============================================================

/// <summary>
/// Request to create a new organization request (e.g., join organization)
/// </summary>
public record CreateRequestRequest
{
    public required string RequestType { get; init; } // JoinOrganization, DepartmentChange, RoleChange, EventParticipation, Other
    public string? Title { get; init; }
    public required string Content { get; init; }
    public Guid? DesiredDepartmentId { get; init; }
    public string? DesiredPosition { get; init; }
}

/// <summary>
/// Request to review (approve/reject) a request
/// </summary>
public record ReviewRequestRequest
{
    public required string Decision { get; init; } // "Approved" or "Rejected"
    public string? ReviewNote { get; init; }
}

// ============================================================
// RESPONSE DTOs
// ============================================================

/// <summary>
/// Request DTO for organization requests
/// </summary>
public record RequestDto
{
    public required Guid Id { get; init; }
    public required Guid SenderId { get; init; }
    public required string SenderName { get; init; }
    public string? SenderEmail { get; init; }
    public required Guid OrganizationId { get; init; }
    public required string OrganizationName { get; init; }
    public required string RequestType { get; init; }
    public string? Title { get; init; }
    public required string Content { get; init; }
    public Guid? DesiredDepartmentId { get; init; }
    public string? DesiredDepartmentName { get; init; }
    public string? DesiredPosition { get; init; }
    public required string Status { get; init; } // Pending, Approved, Rejected, Cancelled, Closed
    public string? ReviewNote { get; init; }
    public Guid? ReviewedByMemberId { get; init; }
    public string? ReviewedByMemberName { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
