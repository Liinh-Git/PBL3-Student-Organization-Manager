namespace Org.Backend.Domain.Entities;

/// <summary>
/// Staff member assigned to help run an event.
/// eventRole is a free-text role (e.g., "MC", "Logistics").
/// </summary>
public class EventMember : BaseEntity
{
    public Guid EventId { get; set; }
    public Guid MemberId { get; set; }
    public string? EventRole { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Event Event { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
