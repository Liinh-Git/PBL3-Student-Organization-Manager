using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// A physical or digital resource owned by an organization.
/// Can optionally be linked to an event when borrowed/used.
/// </summary>
public class Resource : BaseEntity
{
    public Guid OrgId { get; set; }
    public Guid? EventId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public string? Type { get; set; }
    public int Quantity { get; set; } = 0;
    public ResourceStatus Status { get; set; } = ResourceStatus.Available;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Event? Event { get; set; }
}
