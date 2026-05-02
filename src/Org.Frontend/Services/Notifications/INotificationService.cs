// ---- Service interface cho quản lý thông báo ----
using Org.Shared.Features.Notifications;

namespace Org.Frontend.Services.Notifications;

public interface INotificationService
{
    Task<GetNotificationsResponse> GetNotificationsAsync(
        int page = 1,
        int pageSize = 20,
        bool? isRead = null,
        string? type = null,
        CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(CancellationToken ct = default);

    Task<NotificationDto> GetNotificationByIdAsync(Guid id, CancellationToken ct = default);

    Task<NotificationDto> MarkAsReadAsync(Guid id, CancellationToken ct = default);

    Task<int> MarkAllAsReadAsync(CancellationToken ct = default);

    Task<int> DeleteNotificationAsync(Guid id, CancellationToken ct = default);

    Task<int> ClearAllNotificationsAsync(bool onlyRead = false, CancellationToken ct = default);

    Task StartRealtimeAsync(CancellationToken ct = default);
    Task StopRealtimeAsync(CancellationToken ct = default);

    // SignalR real-time notification message
    event Action<NotificationMessage>? OnNotificationReceived;
}
