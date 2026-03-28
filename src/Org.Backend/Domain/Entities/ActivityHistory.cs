using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Audit log of notable activities within an organization (timeline/feed).
/// referenceId is a polymorphic FK (eventId, memberId, etc.) depending on type.
/// </summary>
public class ActivityHistory : BaseEntity
{
    public Guid OrgId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? CoverUrl { get; set; }
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;
    public ActivityType Type { get; set; }
    public Guid? ReferenceId { get; set; }  // polymorphic: points to an Event, Member, etc.
    public string? ThumbnailLink { get; set; }
    public bool IsPublic { get; set; } = true;

    // Navigation
    public Organization Organization { get; set; } = null!;
}
