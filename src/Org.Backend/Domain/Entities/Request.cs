using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// A request sent by a user to an organization.
/// Types: JoinClub, ApproveEvent, ResourceBorrow.
/// </summary>
public class Request : BaseEntity
{
    public Guid SenderId { get; set; }    // FK → User
    public Guid OrgId { get; set; }       // FK → Organization
    public RequestType RequestType { get; set; }
    public string? Content { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    // Navigation
    public User Sender { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
}
