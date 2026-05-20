using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Người tham dự/đăng ký/check-in event.
/// Scope: MUST_HAVE_DB_V1.
/// UI/API working: no in base prototype; DB domain foundation only.
/// </summary>
public class Attendee : BaseEntity
{
    public Guid EventId { get; set; }
    public Guid? UserId { get; set; }
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
    public string? GuestPhone { get; set; }
    public AttendeeStatus Status { get; set; } = AttendeeStatus.Registered;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? CheckedInAt { get; set; }
    public string? Note { get; set; }

    // Navigation properties
    public virtual Event Event { get; set; } = null!;
    public virtual User? User { get; set; }
}
