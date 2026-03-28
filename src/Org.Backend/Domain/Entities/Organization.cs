using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Represents a named organization (club, group, etc.).
/// totalMembers is a denormalized count updated via triggers or application logic.
/// </summary>
public class Organization : BaseEntity
{
    public string OrgName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
    public DateTime? FoundingDate { get; set; }
    public string? Location { get; set; }
    public int TotalMembers { get; set; } = 0;
    public OrgStatus Status { get; set; } = OrgStatus.Active;

    // Navigation
    public ICollection<Department> Departments { get; set; } = [];
    public ICollection<Member> Members { get; set; } = [];
    public ICollection<Event> Events { get; set; } = [];
    public ICollection<Role> Roles { get; set; } = [];
    public ICollection<Resource> Resources { get; set; } = [];
    public ICollection<Request> Requests { get; set; } = [];
    public ICollection<ActivityHistory> ActivityHistories { get; set; } = [];
}
