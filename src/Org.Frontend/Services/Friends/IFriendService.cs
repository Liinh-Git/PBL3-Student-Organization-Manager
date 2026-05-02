namespace Org.Frontend.Services.Friends;

public interface IFriendService
{
    Task<IReadOnlyList<FriendProfileItem>> GetFriendsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<FriendProfileItem>> GetDiscoverUsersAsync(int take = 12, CancellationToken ct = default);
    Task<IReadOnlyList<FriendRequestItem>> GetIncomingRequestsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<FriendRequestItem>> GetOutgoingRequestsAsync(CancellationToken ct = default);
    Task SendRequestAsync(Guid receiverId, string? message = null, CancellationToken ct = default);
    Task AcceptRequestAsync(Guid requestId, CancellationToken ct = default);
    Task RejectRequestAsync(Guid requestId, CancellationToken ct = default);
    Task RemoveFriendAsync(Guid friendUserId, CancellationToken ct = default);
}

public sealed record FriendProfileItem(
    Guid UserId,
    string FullName,
    string? AvatarUrl,
    string? Bio);

public sealed record FriendRequestItem(
    Guid RequestId,
    Guid SenderId,
    string SenderName,
    string? SenderAvatarUrl,
    Guid ReceiverId,
    string ReceiverName,
    string? ReceiverAvatarUrl,
    string Status,
    string? Message,
    DateTime CreatedAt);
