namespace Org.Frontend.Services.Friends;

public sealed class FriendApiClient : IFriendService
{
    private static NotSupportedException BuildNotSupported()
        => new("Friend API chua san sang o backend. Vui long bat mock mode hoac bo sung endpoint friends.");

    public Task<IReadOnlyList<FriendProfileItem>> GetFriendsAsync(CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task<IReadOnlyList<FriendProfileItem>> GetDiscoverUsersAsync(int take = 12, CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task<IReadOnlyList<FriendRequestItem>> GetIncomingRequestsAsync(CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task<IReadOnlyList<FriendRequestItem>> GetOutgoingRequestsAsync(CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task SendRequestAsync(Guid receiverId, string? message = null, CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task AcceptRequestAsync(Guid requestId, CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task RejectRequestAsync(Guid requestId, CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task RemoveFriendAsync(Guid friendUserId, CancellationToken ct = default)
        => throw BuildNotSupported();
}
