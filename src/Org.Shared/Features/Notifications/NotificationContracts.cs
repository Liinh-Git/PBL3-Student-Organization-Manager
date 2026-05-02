// ---- DTO và request dùng chung giữa FE và BE cho module thông báo ----
namespace Org.Shared.Features.Notifications;

// ---- Actor DTO (người tạo thông báo) ----
public sealed record NotificationActorDto(
    Guid Id,
    string FullName,
    string? AvatarUrl);

// ---- Notification DTO ----
public sealed record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime? ReadAt,
    string? ActionUrl,
    string? IconUrl,
    NotificationActorDto? Actor,
    DateTimeOffset CreatedAt);

// ---- GET /api/notifications — lấy danh sách thông báo ----
public sealed record GetNotificationsRequest(
    int Page = 1,
    int PageSize = 20,
    bool? IsRead = null,
    string? Type = null);

public sealed record GetNotificationsResponse(
    IReadOnlyList<NotificationDto> Items,
    int TotalCount,
    int UnreadCount,
    int Page,
    int PageSize);

// ---- GET /api/notifications/unread-count — số lượng chưa đọc ----
public sealed record GetUnreadCountResponse(int Count);

// ---- GET /api/notifications/{id} — chi tiết thông báo ----
public sealed record GetNotificationByIdResponse(NotificationDto Data);

// ---- PUT /api/notifications/{id}/read — đánh dấu đã đọc ----
public sealed record MarkAsReadResponse(NotificationDto Data);

// ---- PUT /api/notifications/read-all — đánh dấu tất cả đã đọc ----
public sealed record MarkAllAsReadResponse(int UpdatedCount);

// ---- DELETE /api/notifications/clear-all — xóa tất cả thông báo ----
public sealed record ClearNotificationsRequest(bool OnlyRead = false);
public sealed record ClearNotificationsResponse(int DeletedCount);

// ---- SignalR real-time notification message ----
/// <summary>
/// Real-time notification message broadcast via SignalR.
/// Sent to clients immediately after notification is created in database.
/// </summary>
public sealed record NotificationMessage(
    Guid Id,
    string Title,
    string Message,
    string Type,
    Guid? ActorId,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    string? ActionUrl,
    string? IconUrl,
    DateTimeOffset Timestamp);
