# Notifications Services

## INotificationService / NotificationService
**Methods**:
- `Task<List<NotificationDto>> ListNotificationsAsync(Guid userId, int page, int pageSize)`
- `Task<int> GetUnreadCountAsync(Guid userId)`
- `Task MarkAsReadAsync(Guid notificationId, Guid userId)`
- `Task MarkAllAsReadAsync(Guid userId)`

## NOT Implemented in Phase 3C
- ❌ No real service implementations
