namespace Org.Backend.Domain.Enums;

/// <summary>
/// Phân loại notification.
/// Storage: string.
/// </summary>
public enum NotificationType
{
    System,
    RequestSubmitted,
    RequestReviewed,
    FriendRequest,
    EventCreated,
    EventUpdated,
    EventReminder,
    TaskAssigned,
    TaskDue,
    ResourceChanged
}
