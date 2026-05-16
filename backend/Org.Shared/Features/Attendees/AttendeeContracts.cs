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
