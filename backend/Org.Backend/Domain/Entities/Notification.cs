using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Notification in-app cho user.
/// Scope: MUST_HAVE_DB_V1.
/// </summary>
public class Notification : BaseEntity
{
    public Guid ReceiverId { get; set; }
    public Guid? ActorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public virtual User Receiver { get; set; } = null!;
    public virtual User? Actor { get; set; }
}
