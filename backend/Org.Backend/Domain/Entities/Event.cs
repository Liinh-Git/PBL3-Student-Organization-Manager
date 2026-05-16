using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Aggregate sự kiện của organization.
/// Scope: MUST_HAVE_DB_V1.
/// </summary>
public class Event : BaseEntity
{
    public Guid OrgId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? Budget { get; set; }
    public string? Location { get; set; }
    public string? BannerUrl { get; set; }
    public int? TargetParticipants { get; set; }
    public string? Tags { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;
    public EventVisibility Visibility { get; set; } = EventVisibility.Private;
    public double? AverageRating { get; set; }
    public Guid? CreatedByMemberId { get; set; }

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual Member? CreatedByMember { get; set; }
    public virtual ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    public virtual ICollection<EventMember> EventMembers { get; set; } = new List<EventMember>();
    public virtual ICollection<Attendee> Attendees { get; set; } = new List<Attendee>();
    public virtual ICollection<DigitalAsset> DigitalAssets { get; set; } = new List<DigitalAsset>();
    public virtual ICollection<EventRating> EventRatings { get; set; } = new List<EventRating>();
    public virtual EventReport? EventReport { get; set; }
    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
