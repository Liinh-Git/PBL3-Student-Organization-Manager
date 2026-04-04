using Org.Backend.Domain.Enums;
using Org.Shared;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// A milestone (phase/checkpoint) within an event.
/// Categories within a milestone group tasks by workstream.
/// orderIndex controls display order.
/// </summary>
public class Milestone : BaseEntity
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; } = 0;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.NotStarted;

    // Navigation
    public Event Event { get; set; } = null!;
    public ICollection<EventCategory> Categories { get; set; } = [];
}
