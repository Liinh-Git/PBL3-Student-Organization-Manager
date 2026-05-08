using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Features.Friends.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.Friends;

namespace Org.Backend.Features.Friends.Services;

public class FriendService : IFriendService
{
    private readonly AppDbContext _context;

    public FriendService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FriendDto>> GetFriendsAsync(Guid userId, CancellationToken ct = default)
    {
        // Get accepted friend requests where user is either sender or receiver
        var acceptedRequests = await _context.FriendRequests
            .Include(fr => fr.Sender)
            .Include(fr => fr.Receiver)
            .Where(fr => (fr.SenderId == userId || fr.ReceiverId == userId) && fr.Status == FriendRequestStatus.Accepted)
            .ToListAsync(ct);

        var friends = new List<FriendDto>();

        foreach (var request in acceptedRequests)
        {
            // Get the other user (not current user)
            var friendUser = request.SenderId == userId ? request.Receiver : request.Sender;
            var friendsSince = request.RespondedAt ?? request.CreatedAt;
            
            friends.Add(friendUser.ToFriendDto(friendsSince));
        }

        return friends.OrderBy(f => f.FullName).ToList();
    }

    public async Task<List<FriendRequestDto>> GetFriendRequestsAsync(Guid userId, CancellationToken ct = default)
    {
        // Get pending friend requests where user is the receiver
        var requests = await _context.FriendRequests
            .Include(fr => fr.Sender)
            .Include(fr => fr.Receiver)
            .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
            .OrderByDescending(fr => fr.CreatedAt)
            .ToListAsync(ct);

        return requests.Select(fr => fr.ToFriendRequestDto()).ToList();
    }

    public async Task<FriendRequestDto> SendFriendRequestAsync(Guid userId, SendFriendRequestRequest request, CancellationToken ct = default)
    {
        // Validate receiver exists
        var receiver = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.ReceiverId, ct);

        if (receiver == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Cannot send request to self
        if (request.ReceiverId == userId)
        {
            throw new InvalidOperationException("Cannot send friend request to yourself");
        }

        // Check if already friends
        var existingFriendship = await _context.FriendRequests
            .FirstOrDefaultAsync(fr => 
                ((fr.SenderId == userId && fr.ReceiverId == request.ReceiverId) ||
                 (fr.SenderId == request.ReceiverId && fr.ReceiverId == userId)) &&
                fr.Status == FriendRequestStatus.Accepted, ct);

        if (existingFriendship != null)
        {
            throw new InvalidOperationException("You are already friends with this user");
        }

        // Check for pending request
        var existingRequest = await _context.FriendRequests
            .FirstOrDefaultAsync(fr => 
                ((fr.SenderId == userId && fr.ReceiverId == request.ReceiverId) ||
                 (fr.SenderId == request.ReceiverId && fr.ReceiverId == userId)) &&
                fr.Status == FriendRequestStatus.Pending, ct);

        if (existingRequest != null)
        {
            throw new InvalidOperationException("A friend request already exists between you and this user");
        }

        // Create friend request
        var friendRequest = new FriendRequest
        {
            SenderId = userId,
            ReceiverId = request.ReceiverId,
            Status = FriendRequestStatus.Pending
        };

        _context.FriendRequests.Add(friendRequest);
        await _context.SaveChangesAsync(ct);

        // Reload with navigation properties
        var createdRequest = await _context.FriendRequests
            .Include(fr => fr.Sender)
            .Include(fr => fr.Receiver)
            .FirstAsync(fr => fr.Id == friendRequest.Id, ct);

        return createdRequest.ToFriendRequestDto();
    }

    public async Task<FriendDto> AcceptFriendRequestAsync(Guid userId, Guid requestId, CancellationToken ct = default)
    {
        var friendRequest = await _context.FriendRequests
            .Include(fr => fr.Sender)
            .Include(fr => fr.Receiver)
            .FirstOrDefaultAsync(fr => fr.Id == requestId, ct);

        if (friendRequest == null)
        {
            throw new KeyNotFoundException("Friend request not found");
        }

        // Verify user is the receiver
        if (friendRequest.ReceiverId != userId)
        {
            throw new UnauthorizedAccessException("You can only accept friend requests sent to you");
        }

        // Verify request is pending
        if (friendRequest.Status != FriendRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot accept friend request with status: {friendRequest.Status}");
        }

        // Accept request
        friendRequest.Status = FriendRequestStatus.Accepted;
        friendRequest.RespondedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        // Return the friend (sender)
        return friendRequest.Sender.ToFriendDto(friendRequest.RespondedAt);
    }

    public async Task RejectFriendRequestAsync(Guid userId, Guid requestId, CancellationToken ct = default)
    {
        var friendRequest = await _context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.Id == requestId, ct);

        if (friendRequest == null)
        {
            throw new KeyNotFoundException("Friend request not found");
        }

        // Verify user is the receiver
        if (friendRequest.ReceiverId != userId)
        {
            throw new UnauthorizedAccessException("You can only reject friend requests sent to you");
        }

        // Verify request is pending
        if (friendRequest.Status != FriendRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot reject friend request with status: {friendRequest.Status}");
        }

        // Reject request
        friendRequest.Status = FriendRequestStatus.Rejected;
        friendRequest.RespondedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }
}
