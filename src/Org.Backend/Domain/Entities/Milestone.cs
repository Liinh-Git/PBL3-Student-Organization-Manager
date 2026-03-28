using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// A milestone (phase/checkpoint) within an event.
/// Tasks must be completed before the milestone dueDate.
/// orderIndex controls display order.
/// </summary>
public class Milestone : BaseEntity
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; } = 0;
    public DateTime DueDate { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.NotStarted;

    // Navigation
    public Event Event { get; set; } = null!;
    public ICollection<OrgTask> Tasks { get; set; } = [];
}
