namespace Org.Backend.Domain.Entities;

/// <summary>
/// Represents membership of a User within an Organization.
/// A user can have one role per org, optionally placed in a Department.
/// </summary>
public class Member : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid OrgId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? RoleId { get; set; }
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public Department? Department { get; set; }
    public Role? Role { get; set; }
    public ICollection<EventMember> EventMembers { get; set; } = [];
    public ICollection<OrgTask> AssignedTasks { get; set; } = [];
    public ICollection<DigitalAsset> UploadedAssets { get; set; } = [];
}
