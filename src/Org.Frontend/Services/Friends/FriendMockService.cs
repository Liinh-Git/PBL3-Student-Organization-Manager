using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;

namespace Org.Frontend.Services.Friends;

public sealed class FriendMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : IFriendService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public async Task<IReadOnlyList<FriendProfileItem>> GetFriendsAsync(CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        return await _mockDataStore.UseAsync(data =>
        {
            var friendIds = data.Friendships
                .Where(x => x.UserAId == currentUserId || x.UserBId == currentUserId)
                .Select(x => x.UserAId == currentUserId ? x.UserBId : x.UserAId)
                .Distinct()
                .ToHashSet();

            return data.Users
                .Where(x => friendIds.Contains(x.Id))
                .Select(MapToProfile)
                .OrderBy(x => x.FullName)
                .ToList()
                .AsReadOnly();
        }, ct);
    }

    public async Task<IReadOnlyList<FriendProfileItem>> GetDiscoverUsersAsync(int take = 12, CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        return await _mockDataStore.UseAsync(data =>
        {
            var blockedIds = new HashSet<Guid> { currentUserId };

            foreach (var fs in data.Friendships.Where(x => x.UserAId == currentUserId || x.UserBId == currentUserId))
                blockedIds.Add(fs.UserAId == currentUserId ? fs.UserBId : fs.UserAId);

            foreach (var req in data.FriendRequests.Where(x =>
                         (x.SenderId == currentUserId || x.ReceiverId == currentUserId) &&
                         string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase)))
            {
                blockedIds.Add(req.SenderId == currentUserId ? req.ReceiverId : req.SenderId);
            }

            return data.Users
                .Where(x => !blockedIds.Contains(x.Id))
                .OrderBy(x => x.FullName)
                .Take(Math.Max(1, take))
                .Select(MapToProfile)
                .ToList()
                .AsReadOnly();
        }, ct);
    }

    public async Task<IReadOnlyList<FriendRequestItem>> GetIncomingRequestsAsync(CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        return await _mockDataStore.UseAsync(data =>
        {
            return data.FriendRequests
                .Where(x => x.ReceiverId == currentUserId && string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => MapToRequest(x, data))
                .ToList()
                .AsReadOnly();
        }, ct);
    }

    public async Task<IReadOnlyList<FriendRequestItem>> GetOutgoingRequestsAsync(CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        return await _mockDataStore.UseAsync(data =>
        {
            return data.FriendRequests
                .Where(x => x.SenderId == currentUserId && string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => MapToRequest(x, data))
                .ToList()
                .AsReadOnly();
        }, ct);
    }

    public async Task SendRequestAsync(Guid receiverId, string? message = null, CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        if (currentUserId == receiverId)
            throw new InvalidOperationException("Không thể gửi lời mời kết bạn cho chính mình.");

        await _mockDataStore.UseAsync(data =>
        {
            if (!data.Users.Any(x => x.Id == receiverId))
                throw new InvalidOperationException("Người dùng nhận không tồn tại.");

            var alreadyFriend = data.Friendships.Any(x =>
                (x.UserAId == currentUserId && x.UserBId == receiverId) ||
                (x.UserAId == receiverId && x.UserBId == currentUserId));

            if (alreadyFriend)
                throw new InvalidOperationException("Hai người đã là bạn bè.");

            var existingPending = data.FriendRequests.Any(x =>
                ((x.SenderId == currentUserId && x.ReceiverId == receiverId) ||
                 (x.SenderId == receiverId && x.ReceiverId == currentUserId)) &&
                string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase));

            if (existingPending)
                throw new InvalidOperationException("Đã tồn tại lời mời kết bạn đang chờ xử lý.");

            data.FriendRequests.Add(new MockFriendRequest
            {
                Id = Guid.NewGuid(),
                SenderId = currentUserId,
                ReceiverId = receiverId,
                Status = "Pending",
                Message = message,
                CreatedAt = DateTime.UtcNow
            });
            return 0;
        }, ct);
    }

    public async Task AcceptRequestAsync(Guid requestId, CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        await _mockDataStore.UseAsync(data =>
        {
            var request = data.FriendRequests.FirstOrDefault(x => x.Id == requestId)
                          ?? throw new InvalidOperationException("Không tìm thấy lời mời kết bạn.");

            if (request.ReceiverId != currentUserId)
                throw new InvalidOperationException("Bạn không có quyền chấp nhận lời mời này.");

            if (!string.Equals(request.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Lời mời đã được xử lý trước đó.");

            request.Status = "Accepted";
            request.RespondedAt = DateTime.UtcNow;

            var exists = data.Friendships.Any(x =>
                (x.UserAId == request.SenderId && x.UserBId == request.ReceiverId) ||
                (x.UserAId == request.ReceiverId && x.UserBId == request.SenderId));

            if (!exists)
            {
                data.Friendships.Add(new MockFriendship
                {
                    Id = Guid.NewGuid(),
                    UserAId = request.SenderId,
                    UserBId = request.ReceiverId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return 0;
        }, ct);
    }

    public async Task RejectRequestAsync(Guid requestId, CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        await _mockDataStore.UseAsync(data =>
        {
            var request = data.FriendRequests.FirstOrDefault(x => x.Id == requestId)
                          ?? throw new InvalidOperationException("Không tìm thấy lời mời kết bạn.");

            if (request.ReceiverId != currentUserId)
                throw new InvalidOperationException("Bạn không có quyền từ chối lời mời này.");

            if (!string.Equals(request.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Lời mời đã được xử lý trước đó.");

            request.Status = "Rejected";
            request.RespondedAt = DateTime.UtcNow;
            return 0;
        }, ct);
    }

    public async Task RemoveFriendAsync(Guid friendUserId, CancellationToken ct = default)
    {
        var currentUserId = await GetCurrentUserIdAsync(ct);
        await _mockDataStore.UseAsync(data =>
        {
            data.Friendships.RemoveAll(x =>
                (x.UserAId == currentUserId && x.UserBId == friendUserId) ||
                (x.UserAId == friendUserId && x.UserBId == currentUserId));
            return 0;
        }, ct);
    }

    private async Task<Guid> GetCurrentUserIdAsync(CancellationToken ct)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var value = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(value, out var userId))
            throw new InvalidOperationException("User not authenticated.");
        return userId;
    }

    private static FriendProfileItem MapToProfile(MockUser user)
        => new(user.Id, user.FullName, user.AvatarUrl, user.Bio);

    private static FriendRequestItem MapToRequest(MockFriendRequest request, MockDataSet data)
    {
        var sender = data.Users.First(x => x.Id == request.SenderId);
        var receiver = data.Users.First(x => x.Id == request.ReceiverId);
        return new FriendRequestItem(
            request.Id,
            request.SenderId,
            sender.FullName,
            sender.AvatarUrl,
            request.ReceiverId,
            receiver.FullName,
            receiver.AvatarUrl,
            request.Status,
            request.Message,
            request.CreatedAt);
    }
}
