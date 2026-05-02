using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Friends;
using Org.Frontend.Services.Mocks;

namespace Org.Frontend.Services.Discover;

public sealed class DiscoverMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider,
    IFriendService friendService) : IDiscoverService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;
    private readonly IFriendService _friendService = friendService;

    public async Task<DiscoverFeedViewModel> GetDiscoverFeedAsync(CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        return await _mockDataStore.UseAsync(data =>
        {
            var organizations = data.Organizations
                .OrderByDescending(x => x.TotalMembers)
                .ThenBy(x => x.OrgName, StringComparer.OrdinalIgnoreCase)
                .Select(x => new DiscoverOrganizationItem(
                    x.Id,
                    x.OrgName,
                    x.Description,
                    ResolveOrgImage(x.AvatarUrl, x.CoverUrl),
                    x.Location,
                    x.TotalMembers > 0 ? x.TotalMembers : data.Members.Count(m => m.OrgId == x.Id)))
                .Take(24)
                .ToList();

            var events = data.Events
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x =>
                {
                    var org = data.Organizations.FirstOrDefault(o => o.Id == x.OrgId);
                    return new DiscoverEventItem(
                        x.Id,
                        x.OrgId,
                        x.Name,
                        org?.OrgName ?? "Organization",
                        x.StartDate,
                        x.EndDate,
                        x.StatusLabel,
                        x.Location,
                        ResolveEventImage(x.ImageUrl, org?.CoverUrl, org?.AvatarUrl));
                })
                .Take(24)
                .ToList();

            var relationshipByUserId = BuildRelationshipLookup(data, currentUserId);
            var users = data.Users
                .Where(x => x.Id != currentUserId)
                .OrderBy(x => x.FullName, StringComparer.OrdinalIgnoreCase)
                .Select(x => new DiscoverUserItem(
                    x.Id,
                    x.FullName,
                    x.AvatarUrl,
                    x.Bio,
                    relationshipByUserId.TryGetValue(x.Id, out var state) ? state : "NONE"))
                .Take(36)
                .ToList();

            return new DiscoverFeedViewModel(organizations, events, users);
        }, ct);
    }

    public async Task<DiscoverSearchResultViewModel> SearchDiscoverAsync(string? query, CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        var normalized = query?.Trim() ?? string.Empty;

        return await _mockDataStore.UseAsync(data =>
        {
            var relationshipByUserId = BuildRelationshipLookup(data, currentUserId);

            var organizations = data.Organizations
                .Where(x => string.IsNullOrWhiteSpace(normalized)
                    || ContainsSearch(x.OrgName, normalized)
                    || ContainsSearch(x.Description, normalized)
                    || ContainsSearch(x.Location, normalized))
                .OrderBy(x => x.OrgName, StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .Select(x => new DiscoverOrganizationItem(
                    x.Id,
                    x.OrgName,
                    x.Description,
                    ResolveOrgImage(x.AvatarUrl, x.CoverUrl),
                    x.Location,
                    x.TotalMembers > 0 ? x.TotalMembers : data.Members.Count(m => m.OrgId == x.Id)))
                .ToList();

            var events = data.Events
                .Where(x => string.IsNullOrWhiteSpace(normalized)
                    || ContainsSearch(x.Name, normalized)
                    || ContainsSearch(x.Description, normalized)
                    || ContainsSearch(x.Location, normalized))
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .Select(x =>
                {
                    var org = data.Organizations.FirstOrDefault(o => o.Id == x.OrgId);
                    return new DiscoverEventItem(
                        x.Id,
                        x.OrgId,
                        x.Name,
                        org?.OrgName ?? "Organization",
                        x.StartDate,
                        x.EndDate,
                        x.StatusLabel,
                        x.Location,
                        ResolveEventImage(x.ImageUrl, org?.CoverUrl, org?.AvatarUrl));
                })
                .ToList();

            var users = data.Users
                .Where(x => x.Id != currentUserId)
                .Where(x => string.IsNullOrWhiteSpace(normalized)
                    || ContainsSearch(x.FullName, normalized)
                    || ContainsSearch(x.Email, normalized)
                    || ContainsSearch(x.Bio, normalized))
                .OrderBy(x => x.FullName, StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .Select(x => new DiscoverUserItem(
                    x.Id,
                    x.FullName,
                    x.AvatarUrl,
                    x.Bio,
                    relationshipByUserId.TryGetValue(x.Id, out var state) ? state : "NONE"))
                .ToList();

            return new DiscoverSearchResultViewModel(normalized, organizations, events, users);
        }, ct);
    }

    public async Task<IReadOnlyList<DiscoverFriendItem>> GetFriendsAsync(CancellationToken ct = default)
    {
        var friends = await _friendService.GetFriendsAsync(ct);
        return friends
            .Select(x => new DiscoverFriendItem(
                x.UserId,
                x.FullName,
                x.AvatarUrl,
                x.Bio,
                CanMessage: true))
            .OrderBy(x => x.FullName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<DiscoverFriendRequestItem>> GetFriendRequestsAsync(CancellationToken ct = default)
    {
        var incoming = await _friendService.GetIncomingRequestsAsync(ct);
        return incoming
            .Select(x => new DiscoverFriendRequestItem(
                x.RequestId,
                x.SenderId,
                x.SenderName,
                x.SenderAvatarUrl,
                x.Message,
                DateTime.SpecifyKind(x.CreatedAt, DateTimeKind.Utc)))
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();
    }

    public Task SendFriendRequestAsync(Guid targetUserId, CancellationToken ct = default)
        => _friendService.SendRequestAsync(targetUserId, ct: ct);

    public Task AcceptFriendRequestAsync(Guid requestId, CancellationToken ct = default)
        => _friendService.AcceptRequestAsync(requestId, ct);

    public Task DeclineFriendRequestAsync(Guid requestId, CancellationToken ct = default)
        => _friendService.RejectRequestAsync(requestId, ct);

    private async Task<Guid> GetCurrentUserIdAsync(CancellationToken ct)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var value = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(value, out var userId))
            throw new InvalidOperationException("User not authenticated.");
        return userId;
    }

    private static Dictionary<Guid, string> BuildRelationshipLookup(Services.Mocks.Models.MockDataSet data, Guid currentUserId)
    {
        var lookup = new Dictionary<Guid, string>();

        foreach (var friendship in data.Friendships)
        {
            if (friendship.UserAId == currentUserId)
                lookup[friendship.UserBId] = "FRIEND";
            else if (friendship.UserBId == currentUserId)
                lookup[friendship.UserAId] = "FRIEND";
        }

        foreach (var request in data.FriendRequests.Where(x =>
                     string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase)
                     && (x.SenderId == currentUserId || x.ReceiverId == currentUserId)))
        {
            if (request.SenderId == currentUserId)
                lookup[request.ReceiverId] = "OUTGOING_PENDING";
            if (request.ReceiverId == currentUserId)
                lookup[request.SenderId] = "INCOMING_PENDING";
        }

        return lookup;
    }

    private static bool ContainsSearch(string? source, string query)
        => !string.IsNullOrWhiteSpace(source)
           && source.Contains(query, StringComparison.OrdinalIgnoreCase);

    private static string ResolveOrgImage(string? avatarUrl, string? coverUrl)
    {
        if (!string.IsNullOrWhiteSpace(avatarUrl))
            return avatarUrl;
        if (!string.IsNullOrWhiteSpace(coverUrl))
            return coverUrl;
        return "/images/mockimages/Org1/Avt.jpg";
    }

    private static string ResolveEventImage(string? eventImageUrl, string? coverUrl, string? avatarUrl)
    {
        if (!string.IsNullOrWhiteSpace(eventImageUrl))
            return eventImageUrl;
        if (!string.IsNullOrWhiteSpace(coverUrl))
            return coverUrl;
        if (!string.IsNullOrWhiteSpace(avatarUrl))
            return avatarUrl;
        return "/images/mockimages/Org1/Card1.jpg";
    }
}
