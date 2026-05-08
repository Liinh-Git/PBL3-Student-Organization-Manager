using Org.Backend.Domain.Entities;
using Org.Shared.Features.Friends;

namespace Org.Backend.Features.Friends.Mappings;

public static class FriendMappings
{
    public static FriendDto ToFriendDto(this User user, DateTime? friendsSince = null)
    {
        return new FriendDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status.ToString(),
            FriendsSince = friendsSince
        };
    }

    public static FriendRequestDto ToFriendRequestDto(this FriendRequest friendRequest)
    {
        return new FriendRequestDto
        {
            Id = friendRequest.Id,
            SenderId = friendRequest.SenderId,
            SenderName = friendRequest.Sender?.FullName ?? string.Empty,
            SenderAvatarUrl = friendRequest.Sender?.AvatarUrl,
            ReceiverId = friendRequest.ReceiverId,
            ReceiverName = friendRequest.Receiver?.FullName ?? string.Empty,
            ReceiverAvatarUrl = friendRequest.Receiver?.AvatarUrl,
            Status = friendRequest.Status.ToString(),
            CreatedAtUtc = friendRequest.CreatedAt,
            RespondedAt = friendRequest.RespondedAt
        };
    }
}
