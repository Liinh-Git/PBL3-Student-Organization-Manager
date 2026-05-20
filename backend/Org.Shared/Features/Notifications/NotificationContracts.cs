namespace Org.Shared.Features.Notifications;

// ============================================================
// RESPONSE DTOs
// ============================================================

/// <summary>
/// Notification DTO for user notifications
/// </summary>
public record NotificationDto
{
    public required Guid Id { get; init; }
    public required Guid ReceiverId { get; init; }
    public Guid? ActorId { get; init; }
    public string? ActorName { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required string Type { get; init; } // System, RequestSubmitted, RequestReviewed, FriendRequest, EventCreated, etc.
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string? ActionUrl { get; init; }
    public required bool IsRead { get; init; }
    public DateTime? ReadAt { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}

/// <summary>
/// Unread count response
/// </summary>
public record UnreadCountDto
{
    public required int Count { get; init; }
}
