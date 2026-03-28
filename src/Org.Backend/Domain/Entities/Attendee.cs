using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Person who registered for/attended an event.
/// userId is nullable — supports external guests (non-system users).
/// </summary>
public class Attendee : BaseEntity
{
    public Guid EventId { get; set; }
    public Guid? UserId { get; set; }       // null for external guests
    public string? GuestName { get; set; }
    public string? Email { get; set; }
    public string? TicketType { get; set; }
    public DateTime? CheckInTime { get; set; }
    public AttendeeStatus Status { get; set; } = AttendeeStatus.Registered;

    // Navigation
    public Event Event { get; set; } = null!;
    public User? User { get; set; }
}
