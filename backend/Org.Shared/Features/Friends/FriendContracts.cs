namespace Org.Shared.Features.Friends;

// ============================================================
// REQUEST DTOs
// ============================================================

/// <summary>
/// Request to send a friend request
/// </summary>
public record SendFriendRequestRequest
{
    public required Guid ReceiverId { get; init; }
}

// ============================================================
// RESPONSE DTOs
// ============================================================

/// <summary>
/// Friend DTO for accepted friendships
/// </summary>
public record FriendDto
{
    public required Guid UserId { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public string? AvatarUrl { get; init; }
    public required string Status { get; init; } // Active, Inactive, Suspended
    public DateTime? FriendsSince { get; init; }
}

/// <summary>
/// Friend request DTO for pending/responded friend requests
/// </summary>
public record FriendRequestDto
{
    public required Guid Id { get; init; }
    public required Guid SenderId { get; init; }
    public required string SenderName { get; init; }
    public string? SenderAvatarUrl { get; init; }
    public required Guid ReceiverId { get; init; }
    public required string ReceiverName { get; init; }
    public string? ReceiverAvatarUrl { get; init; }
    public required string Status { get; init; } // Pending, Accepted, Rejected, Cancelled, Blocked
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? RespondedAt { get; init; }
}
