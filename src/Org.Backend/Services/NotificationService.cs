// ---- Implementation của Notification Service ----
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;
using Org.Backend.Hubs;
using Org.Backend.Infrastructure.Database;

namespace Org.Backend.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(AppDbContext db, IHubContext<NotificationHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    // === FRIEND SYSTEM ===
    
    public async Task NotifyFriendRequestReceived(Guid receiverId, Guid senderId, Guid friendRequestId)
    {
        var sender = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == senderId);
        if (sender is null) return;

        await CreateNotification(
            receiverId,
            "Lời mời kết bạn mới",
            $"{sender.FullName} đã gửi lời mời kết bạn cho bạn.",
            NotificationType.FriendRequestReceived,
            senderId,
            friendRequestId,
            "FriendRequest",
            $"/friends/requests",
            sender.AvatarUrl);
    }

    public async Task NotifyFriendRequestAccepted(Guid receiverId, Guid acceptorId)
    {
        var acceptor = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == acceptorId);
        if (acceptor is null) return;

        await CreateNotification(
            receiverId,
            "Lời mời kết bạn được chấp nhận",
            $"{acceptor.FullName} đã chấp nhận lời mời kết bạn của bạn.",
            NotificationType.FriendRequestAccepted,
            acceptorId,
            null,
            null,
            $"/friends",
            acceptor.AvatarUrl);
    }

    public async Task NotifyFriendRequestRejected(Guid receiverId, Guid rejectorId)
    {
        var rejector = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == rejectorId);
        if (rejector is null) return;

        await CreateNotification(
            receiverId,
            "Lời mời kết bạn bị từ chối",
            $"{rejector.FullName} đã từ chối lời mời kết bạn của bạn.",
            NotificationType.FriendRequestRejected,
            rejectorId,
            null,
            null,
            null,
            rejector.AvatarUrl);
    }

    // === ORGANIZATION MEMBERSHIP ===
    
    public async Task NotifyJoinRequestReceived(Guid managerId, Guid requesterId, Guid orgId, Guid requestId)
    {
        var requester = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == requesterId);
        var org = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orgId);
        if (requester is null || org is null) return;

        await CreateNotification(
            managerId,
            "Yêu cầu tham gia mới",
            $"{requester.FullName} muốn tham gia {org.OrgName}.",
            NotificationType.JoinRequestReceived,
            requesterId,
            requestId,
            "Request",
            $"/organizations/{orgId}/requests",
            requester.AvatarUrl);
    }

    public async Task NotifyJoinRequestApproved(Guid userId, Guid approverId, Guid orgId, Guid memberId)
    {
        var approver = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == approverId);
        var org = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orgId);
        if (approver is null || org is null) return;

        await CreateNotification(
            userId,
            "Yêu cầu tham gia được duyệt",
            $"Yêu cầu tham gia {org.OrgName} của bạn đã được {approver.FullName} chấp nhận.",
            NotificationType.JoinRequestApproved,
            approverId,
            memberId,
            "Member",
            $"/organizations/{orgId}",
            org.AvatarUrl);
    }

    public async Task NotifyJoinRequestRejected(Guid userId, Guid rejectorId, Guid orgId)
    {
        var rejector = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == rejectorId);
        var org = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orgId);
        if (rejector is null || org is null) return;

        await CreateNotification(
            userId,
            "Yêu cầu tham gia bị từ chối",
            $"Yêu cầu tham gia {org.OrgName} của bạn đã bị từ chối.",
            NotificationType.JoinRequestRejected,
            rejectorId,
            null,
            null,
            null,
            org.AvatarUrl);
    }

    public async Task NotifyMemberRoleChanged(Guid memberId, Guid changedBy, Guid orgId, string newRole)
    {
        var member = await _db.Members.AsNoTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == memberId);
        var changer = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == changedBy);
        var org = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orgId);
        if (member is null || changer is null || org is null) return;

        await CreateNotification(
            member.UserId,
            "Vai trò thay đổi",
            $"Vai trò của bạn trong {org.OrgName} đã được thay đổi thành {newRole}.",
            NotificationType.MemberRoleChanged,
            changedBy,
            memberId,
            "Member",
            $"/organizations/{orgId}",
            org.AvatarUrl);
    }

    public async Task NotifyMemberRemoved(Guid userId, Guid removedBy, Guid orgId)
    {
        var remover = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == removedBy);
        var org = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orgId);
        if (remover is null || org is null) return;

        await CreateNotification(
            userId,
            "Bị xóa khỏi tổ chức",
            $"Bạn đã bị xóa khỏi {org.OrgName}.",
            NotificationType.MemberRemoved,
            removedBy,
            null,
            null,
            null,
            org.AvatarUrl);
    }

    // === EVENT SYSTEM ===
    
    public async Task NotifyEventInvitation(Guid userId, Guid invitedBy, Guid eventId)
    {
        var inviter = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == invitedBy);
        var evt = await _db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == eventId);
        if (inviter is null || evt is null) return;

        await CreateNotification(
            userId,
            "Lời mời tham gia sự kiện",
            $"{inviter.FullName} mời bạn tham gia sự kiện {evt.EventName}.",
            NotificationType.EventInvitation,
            invitedBy,
            eventId,
            "Event",
            $"/events/{eventId}",
            null);
    }

    public async Task NotifyEventRegistrationApproved(Guid userId, Guid approverId, Guid eventId)
    {
        var approver = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == approverId);
        var evt = await _db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == eventId);
        if (approver is null || evt is null) return;

        await CreateNotification(
            userId,
            "Đăng ký sự kiện được duyệt",
            $"Đăng ký tham gia sự kiện {evt.EventName} của bạn đã được chấp nhận.",
            NotificationType.EventRegistrationApproved,
            approverId,
            eventId,
            "Event",
            $"/events/{eventId}",
            null);
    }

    public async Task NotifyEventRegistrationRejected(Guid userId, Guid rejectorId, Guid eventId)
    {
        var rejector = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == rejectorId);
        var evt = await _db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == eventId);
        if (rejector is null || evt is null) return;

        await CreateNotification(
            userId,
            "Đăng ký sự kiện bị từ chối",
            $"Đăng ký tham gia sự kiện {evt.EventName} của bạn đã bị từ chối.",
            NotificationType.EventRegistrationRejected,
            rejectorId,
            eventId,
            "Event",
            null,
            null);
    }

    public async Task NotifyEventUpdated(Guid userId, Guid updatedBy, Guid eventId)
    {
        var updater = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == updatedBy);
        var evt = await _db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == eventId);
        if (updater is null || evt is null) return;

        await CreateNotification(
            userId,
            "Sự kiện có thay đổi",
            $"Sự kiện {evt.EventName} đã được cập nhật. Vui lòng kiểm tra lại thông tin.",
            NotificationType.EventUpdated,
            updatedBy,
            eventId,
            "Event",
            $"/events/{eventId}",
            null);
    }

    public async Task NotifyEventCancelled(Guid userId, Guid cancelledBy, Guid eventId)
    {
        var canceller = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cancelledBy);
        var evt = await _db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == eventId);
        if (canceller is null || evt is null) return;

        await CreateNotification(
            userId,
            "Sự kiện bị hủy",
            $"Sự kiện {evt.EventName} đã bị hủy.",
            NotificationType.EventCancelled,
            cancelledBy,
            eventId,
            "Event",
            null,
            null);
    }

    // === TASK SYSTEM ===
    
    public async Task NotifyTaskAssigned(Guid assigneeId, Guid assignedBy, Guid taskId)
    {
        var assigner = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == assignedBy);
        var task = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == taskId);
        if (assigner is null || task is null) return;

        await CreateNotification(
            assigneeId,
            "Nhiệm vụ mới",
            $"{assigner.FullName} đã giao nhiệm vụ \"{task.TaskName}\" cho bạn.",
            NotificationType.TaskAssigned,
            assignedBy,
            taskId,
            "Task",
            $"/tasks/{taskId}",
            null);
    }

    public async Task NotifyTaskStatusChanged(Guid userId, Guid changedBy, Guid taskId, string newStatus)
    {
        var changer = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == changedBy);
        var task = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == taskId);
        if (changer is null || task is null) return;

        await CreateNotification(
            userId,
            "Trạng thái nhiệm vụ thay đổi",
            $"Nhiệm vụ \"{task.TaskName}\" đã chuyển sang trạng thái {newStatus}.",
            NotificationType.TaskStatusChanged,
            changedBy,
            taskId,
            "Task",
            $"/tasks/{taskId}",
            null);
    }

    // === GENERAL ===
    
    public async Task CreateNotification(
        Guid receiverId,
        string title,
        string message,
        NotificationType type,
        Guid? actorId = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        string? actionUrl = null,
        string? iconUrl = null)
    {
        // 1. Create and save notification to database
        var notification = new Notification
        {
            ReceiverId = receiverId,
            Title = title,
            Message = message,
            Type = type,
            ActorId = actorId,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            ActionUrl = actionUrl,
            IconUrl = iconUrl,
            IsRead = false
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        // 2. Broadcast via SignalR (fire-and-forget)
        try
        {
            await _hubContext.Clients
                .User(receiverId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type.ToString(),
                    actorId = notification.ActorId,
                    relatedEntityId = notification.RelatedEntityId,
                    relatedEntityType = notification.RelatedEntityType,
                    actionUrl = notification.ActionUrl,
                    iconUrl = notification.IconUrl,
                    timestamp = new DateTimeOffset(
                        DateTime.SpecifyKind(notification.CreatedAt, DateTimeKind.Utc))
                });
        }
        catch (Exception ex)
        {
            // Log error but don't throw - notification is already persisted
            // User will see it when they refresh or poll
            Console.WriteLine($"SignalR broadcast failed for notification {notification.Id}: {ex.Message}");
        }
    }
}
