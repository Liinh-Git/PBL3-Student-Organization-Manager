namespace Org.Frontend.Services.Discover;

public sealed class DiscoverApiClient : IDiscoverService
{
    public DiscoverApiClient(System.Net.Http.HttpClient httpClient)
    {
    }

    private static NotSupportedException BuildNotSupported()
        => new("Discover/Friends API chua san sang o backend. Vui long bat mock mode hoac bo sung endpoint discover.");

    public Task<DiscoverFeedViewModel> GetDiscoverFeedAsync(CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task<DiscoverSearchResultViewModel> SearchDiscoverAsync(string? query, CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task<IReadOnlyList<DiscoverFriendItem>> GetFriendsAsync(CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task<IReadOnlyList<DiscoverFriendRequestItem>> GetFriendRequestsAsync(CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task SendFriendRequestAsync(Guid targetUserId, CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task AcceptFriendRequestAsync(Guid requestId, CancellationToken ct = default)
        => throw BuildNotSupported();

    public Task DeclineFriendRequestAsync(Guid requestId, CancellationToken ct = default)
        => throw BuildNotSupported();
}
