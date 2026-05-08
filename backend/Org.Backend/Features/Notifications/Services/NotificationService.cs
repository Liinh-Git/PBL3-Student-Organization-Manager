using Microsoft.EntityFrameworkCore;
using Org.Backend.Features.Notifications.Mappings;
using Org.Backend.Infrastructure.Persistence;
using Org.Shared.Features.Notifications;

namespace Org.Backend.Features.Notifications.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, CancellationToken ct = default)
    {
        var notifications = await _context.Notifications
            .Include(n => n.Actor)
            .Where(n => n.ReceiverId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

        return notifications.Select(n => n.ToNotificationDto()).ToList();
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        var count = await _context.Notifications
            .Where(n => n.ReceiverId == userId && !n.IsRead)
            .CountAsync(ct);

        return new UnreadCountDto { Count = count };
    }

    public async Task<NotificationDto> MarkNotificationReadAsync(Guid userId, Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _context.Notifications
            .Include(n => n.Actor)
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);

        if (notification == null)
        {
            throw new KeyNotFoundException("Notification not found");
        }

        // Verify notification belongs to current user
        if (notification.ReceiverId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this notification");
        }

        // Mark as read if not already read
        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        return notification.ToNotificationDto();
    }

    public async Task MarkAllNotificationsReadAsync(Guid userId, CancellationToken ct = default)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.ReceiverId == userId && !n.IsRead)
            .ToListAsync(ct);

        if (unreadNotifications.Any())
        {
            var now = DateTime.UtcNow;
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}
