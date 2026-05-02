namespace Org.Frontend.Services.Discover;

public interface IDiscoverService
{
    Task<DiscoverFeedViewModel> GetDiscoverFeedAsync(CancellationToken ct = default);
    Task<DiscoverSearchResultViewModel> SearchDiscoverAsync(string? query, CancellationToken ct = default);
    Task<IReadOnlyList<DiscoverFriendItem>> GetFriendsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DiscoverFriendRequestItem>> GetFriendRequestsAsync(CancellationToken ct = default);
    Task SendFriendRequestAsync(Guid targetUserId, CancellationToken ct = default);
    Task AcceptFriendRequestAsync(Guid requestId, CancellationToken ct = default);
    Task DeclineFriendRequestAsync(Guid requestId, CancellationToken ct = default);
}

public sealed record DiscoverFeedViewModel(
    IReadOnlyList<DiscoverOrganizationItem> Organizations,
    IReadOnlyList<DiscoverEventItem> Events,
    IReadOnlyList<DiscoverUserItem> Users);

public sealed record DiscoverSearchResultViewModel(
    string Query,
    IReadOnlyList<DiscoverOrganizationItem> Organizations,
    IReadOnlyList<DiscoverEventItem> Events,
    IReadOnlyList<DiscoverUserItem> Users);

public sealed record DiscoverOrganizationItem(
    Guid OrganizationId,
    string Name,
    string? Description,
    string? AvatarUrl,
    string? Location,
    int MemberCount);

public sealed record DiscoverEventItem(
    Guid EventId,
    Guid OrganizationId,
    string Title,
    string OrganizationName,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Status,
    string? Location,
    string? ImageUrl);

public sealed record DiscoverUserItem(
    Guid UserId,
    string FullName,
    string? AvatarUrl,
    string? Subtitle,
    string RelationshipState);

public sealed record DiscoverFriendItem(
    Guid UserId,
    string FullName,
    string? AvatarUrl,
    string? Subtitle,
    bool CanMessage);

public sealed record DiscoverFriendRequestItem(
    Guid RequestId,
    Guid RequesterId,
    string RequesterName,
    string? RequesterAvatarUrl,
    string? Context,
    DateTime CreatedAtUtc);
