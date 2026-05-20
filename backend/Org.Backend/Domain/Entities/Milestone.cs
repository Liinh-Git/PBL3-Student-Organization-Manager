using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Phân chặng trong event.
/// Scope: MUST_HAVE_DB_V1.
/// </summary>
public class Milestone : BaseEntity
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Planned;

    // Navigation properties
    public virtual Event Event { get; set; } = null!;
    public virtual ICollection<EventCategory> Categories { get; set; } = new List<EventCategory>();
}
