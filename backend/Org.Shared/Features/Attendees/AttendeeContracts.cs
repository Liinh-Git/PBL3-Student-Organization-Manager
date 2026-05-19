namespace Org.Shared.Features.Attendees;

public record AttendeeDto
{
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public Guid? UserId { get; init; }
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public required string Status { get; init; }
    public required DateTime RegisteredAtUtc { get; init; }
    public DateTime? CheckedInAtUtc { get; init; }
    public string? Note { get; init; }
}

public record RegisterEventAttendeeRequest
{
    public string? Note { get; init; }
}

public record CancelEventRegistrationRequest
{
    public string? Note { get; init; }
}

/// <summary>
/// Current user's registration snapshot for a specific event.
/// </summary>
public record AttendeeRegistrationDto
{
    public required Guid EventId { get; init; }
    public required Guid UserId { get; init; }
    public bool IsEventMember { get; init; }
    public required bool IsRegistered { get; init; }
    public Guid? AttendeeId { get; init; }
    public string? Status { get; init; }
    public DateTime? RegisteredAtUtc { get; init; }
    public DateTime? CheckedInAtUtc { get; init; }
}

/// <summary>
/// Update payload when current user unregisters from an event.
/// </summary>
public record AttendeeRegistrationUpdateDto
{
    public string? Note { get; init; }
}

public record RequestCheckInRequest
{
    public string? Note { get; init; }
}

public record ReviewCheckInRequest
{
    public required bool Approve { get; init; }
    public string? Note { get; init; }
}
