namespace Org.Backend.Domain.Enums;

/// <summary>
/// Trạng thái friend request.
/// Storage: string.
/// </summary>
public enum FriendRequestStatus
{
    Pending,
    Accepted,
    Rejected,
    Cancelled,
    Blocked
}
