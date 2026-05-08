using Org.Shared.Features.Friends;

namespace Org.Backend.Features.Friends.Services;

public interface IFriendService
{
    Task<List<FriendDto>> GetFriendsAsync(Guid userId, CancellationToken ct = default);
    Task<List<FriendRequestDto>> GetFriendRequestsAsync(Guid userId, CancellationToken ct = default);
    Task<FriendRequestDto> SendFriendRequestAsync(Guid userId, SendFriendRequestRequest request, CancellationToken ct = default);
    Task<FriendDto> AcceptFriendRequestAsync(Guid userId, Guid requestId, CancellationToken ct = default);
    Task RejectFriendRequestAsync(Guid userId, Guid requestId, CancellationToken ct = default);
}
