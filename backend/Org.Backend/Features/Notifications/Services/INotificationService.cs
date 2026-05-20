using Org.Shared.Features.Notifications;

namespace Org.Backend.Features.Notifications.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, CancellationToken ct = default);
    Task<UnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task<NotificationDto> MarkNotificationReadAsync(Guid userId, Guid notificationId, CancellationToken ct = default);
    Task MarkAllNotificationsReadAsync(Guid userId, CancellationToken ct = default);
}
