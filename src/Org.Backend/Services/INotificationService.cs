// ---- Interface cho Notification Service ----
using Org.Backend.Domain.Enums;

namespace Org.Backend.Services;

/// <summary>
/// Service để tạo và gửi thông báo tự động.
/// Sử dụng trong các endpoints để notify users khi có sự kiện xảy ra.
/// </summary>
public interface INotificationService
{
    // === FRIEND SYSTEM ===
    Task NotifyFriendRequestReceived(Guid receiverId, Guid senderId, Guid friendRequestId);
    Task NotifyFriendRequestAccepted(Guid receiverId, Guid acceptorId);
    Task NotifyFriendRequestRejected(Guid receiverId, Guid rejectorId);
    
    // === ORGANIZATION MEMBERSHIP ===
    Task NotifyJoinRequestReceived(Guid managerId, Guid requesterId, Guid orgId, Guid requestId);
    Task NotifyJoinRequestApproved(Guid userId, Guid approverId, Guid orgId, Guid memberId);
    Task NotifyJoinRequestRejected(Guid userId, Guid rejectorId, Guid orgId);
    Task NotifyMemberRoleChanged(Guid memberId, Guid changedBy, Guid orgId, string newRole);
    Task NotifyMemberRemoved(Guid userId, Guid removedBy, Guid orgId);
    
    // === EVENT SYSTEM ===
    Task NotifyEventInvitation(Guid userId, Guid invitedBy, Guid eventId);
    Task NotifyEventRegistrationApproved(Guid userId, Guid approverId, Guid eventId);
    Task NotifyEventRegistrationRejected(Guid userId, Guid rejectorId, Guid eventId);
    Task NotifyEventUpdated(Guid userId, Guid updatedBy, Guid eventId);
    Task NotifyEventCancelled(Guid userId, Guid cancelledBy, Guid eventId);
    
    // === TASK SYSTEM ===
    Task NotifyTaskAssigned(Guid assigneeId, Guid assignedBy, Guid taskId);
    Task NotifyTaskStatusChanged(Guid userId, Guid changedBy, Guid taskId, string newStatus);
    
    // === GENERAL ===
    Task CreateNotification(
        Guid receiverId,
        string title,
        string message,
        NotificationType type,
        Guid? actorId = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        string? actionUrl = null,
        string? iconUrl = null);
}
