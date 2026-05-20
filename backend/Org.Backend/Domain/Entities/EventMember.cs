using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Staff/organizer nội bộ của event.
/// Scope: MUST_HAVE_DB_V1.
/// UI/API working: no in base prototype; DB domain foundation only.
/// </summary>
public class EventMember : BaseEntity
{
    public Guid EventId { get; set; }
    public Guid MemberId { get; set; }
    public EventRole EventRole { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }

    // Navigation properties
    public virtual Event Event { get; set; } = null!;
    public virtual Member Member { get; set; } = null!;
}
