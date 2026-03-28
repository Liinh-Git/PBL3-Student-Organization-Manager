using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// An event organized by an Organization.
/// tags stored as JSON string array. averageRating computed from EventReport.
/// </summary>
public class Event : BaseEntity
{
    public Guid OrgId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Budget { get; set; } = 0;
    public string? Location { get; set; }
    public int TargetParticipants { get; set; } = 0;
    public string? Tags { get; set; }   // stored as JSON array: ["music","outdoor",...]
    public EventStatus Status { get; set; } = EventStatus.Draft;
    public float AverageRating { get; set; } = 0;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public ICollection<EventMember> EventMembers { get; set; } = [];
    public EventReport? EventReport { get; set; }
    public ICollection<Milestone> Milestones { get; set; } = [];
    public ICollection<Attendee> Attendees { get; set; } = [];
    public ICollection<DigitalAsset> DigitalAssets { get; set; } = [];
    public ICollection<Resource> Resources { get; set; } = [];
}
