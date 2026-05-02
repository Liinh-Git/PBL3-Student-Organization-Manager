// ---- Mock service cho quản lý thông báo ----
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Org.Frontend.Services.Mocks;
using Org.Frontend.Services.Mocks.Models;
using Org.Shared.Features.Notifications;

namespace Org.Frontend.Services.Notifications;

public sealed class NotificationMockService(
    FrontendMockDataStore mockDataStore,
    AuthenticationStateProvider authStateProvider) : INotificationService
{
    private readonly FrontendMockDataStore _mockDataStore = mockDataStore;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

    public event Action<NotificationMessage>? OnNotificationReceived;

    public Task StartRealtimeAsync(CancellationToken ct = default) => Task.CompletedTask;

    public Task StopRealtimeAsync(CancellationToken ct = default) => Task.CompletedTask;

    public async Task<GetNotificationsResponse> GetNotificationsAsync(
        int page = 1,
        int pageSize = 20,
        bool? isRead = null,
        string? type = null,
        CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = Guid.TryParse(userIdText, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;

        if (userId == null)
            return new GetNotificationsResponse([], 0, 0, page, pageSize);

        return await _mockDataStore.UseAsync(data =>
        {
            var notifications = data.Notifications
                .Where(x => x.ReceiverId == userId.Value)
                .AsQueryable();

            // Filter by IsRead
            if (isRead.HasValue)
                notifications = notifications.Where(x => x.IsRead == isRead.Value);

            // Filter by Type
            if (!string.IsNullOrWhiteSpace(type))
                notifications = notifications.Where(x => x.Type == type);

            var totalCount = notifications.Count();
            var unreadCount = notifications.Count(x => !x.IsRead);

            var items = notifications
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new NotificationDto(
                    x.Id,
                    x.Title,
                    x.Message,
                    x.Type,
                    x.IsRead,
                    x.ReadAt,
                    x.ActionUrl,
                    x.IconUrl,
                    x.ActorId.HasValue
                        ? new NotificationActorDto(
                            x.ActorId.Value,
                            data.Users.Where(u => u.Id == x.ActorId.Value).Select(u => u.FullName).FirstOrDefault() ?? "Unknown",
                            data.Users.Where(u => u.Id == x.ActorId.Value).Select(u => u.AvatarUrl).FirstOrDefault())
                        : null,
                    new DateTimeOffset(DateTime.SpecifyKind(x.CreatedAt, DateTimeKind.Utc))))
                .ToList();

            return new GetNotificationsResponse(items, totalCount, unreadCount, page, pageSize);
        }, ct);
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = Guid.TryParse(userIdText, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;

        if (userId == null)
            return 0;

        return await _mockDataStore.UseAsync(data =>
        {
            return data.Notifications.Count(x => x.ReceiverId == userId.Value && !x.IsRead);
        }, ct);
    }

    public async Task<NotificationDto> GetNotificationByIdAsync(Guid id, CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = Guid.TryParse(userIdText, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;

        if (userId == null)
            throw new InvalidOperationException("Notification not found");

        return await _mockDataStore.UseAsync(data =>
        {
            var notification = data.Notifications.FirstOrDefault(x => x.Id == id && x.ReceiverId == userId.Value);
            if (notification == null)
                throw new InvalidOperationException("Notification not found");

            return new NotificationDto(
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.IsRead,
                notification.ReadAt,
                notification.ActionUrl,
                notification.IconUrl,
                notification.ActorId.HasValue
                    ? new NotificationActorDto(
                        notification.ActorId.Value,
                        data.Users.FirstOrDefault(u => u.Id == notification.ActorId.Value)?.FullName ?? "Unknown",
                        data.Users.FirstOrDefault(u => u.Id == notification.ActorId.Value)?.AvatarUrl)
                    : null,
                new DateTimeOffset(DateTime.SpecifyKind(notification.CreatedAt, DateTimeKind.Utc)));
        }, ct);
    }

    public async Task<NotificationDto> MarkAsReadAsync(Guid id, CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = Guid.TryParse(userIdText, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;

        if (userId == null)
            throw new InvalidOperationException("Notification not found");

        return await _mockDataStore.UseAsync(data =>
        {
            var notification = data.Notifications.FirstOrDefault(x => x.Id == id && x.ReceiverId == userId.Value);
            if (notification == null)
                throw new InvalidOperationException("Notification not found");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            return new NotificationDto(
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.IsRead,
                notification.ReadAt,
                notification.ActionUrl,
                notification.IconUrl,
                notification.ActorId.HasValue
                    ? new NotificationActorDto(
                        notification.ActorId.Value,
                        data.Users.FirstOrDefault(u => u.Id == notification.ActorId.Value)?.FullName ?? "Unknown",
                        data.Users.FirstOrDefault(u => u.Id == notification.ActorId.Value)?.AvatarUrl)
                    : null,
                new DateTimeOffset(DateTime.SpecifyKind(notification.CreatedAt, DateTimeKind.Utc)));
        }, ct);
    }

    public async Task<int> MarkAllAsReadAsync(CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = Guid.TryParse(userIdText, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;

        if (userId == null)
            return 0;

        return await _mockDataStore.UseAsync(data =>
        {
            var count = 0;
            foreach (var notification in data.Notifications)
            {
                if (notification.ReceiverId == userId.Value && !notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    count++;
                }
            }
            return count;
        }, ct);
    }

    public async Task<int> DeleteNotificationAsync(Guid id, CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = Guid.TryParse(userIdText, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;

        if (userId == null)
            return 0;

        return await _mockDataStore.UseAsync(data =>
        {
            var notification = data.Notifications.FirstOrDefault(x => x.Id == id && x.ReceiverId == userId.Value);
            if (notification != null)
            {
                data.Notifications.Remove(notification);
                return 1;
            }
            return 0;
        }, ct);
    }

    public async Task<int> ClearAllNotificationsAsync(bool onlyRead = false, CancellationToken ct = default)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var userIdText = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = Guid.TryParse(userIdText, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;

        if (userId == null)
            return 0;

        return await _mockDataStore.UseAsync(data =>
        {
            var count = 0;
            if (onlyRead)
            {
                var readNotifications = data.Notifications
                    .Where(x => x.ReceiverId == userId.Value && x.IsRead)
                    .ToList();
                count = readNotifications.Count;
                foreach (var notification in readNotifications)
                {
                    data.Notifications.Remove(notification);
                }
            }
            else
            {
                count = data.Notifications.Count(x => x.ReceiverId == userId.Value);
                data.Notifications.RemoveAll(x => x.ReceiverId == userId.Value);
            }
            return count;
        }, ct);
    }

    // Method to handle SignalR notification received
    public void HandleNotificationReceived(NotificationMessage notification)
    {
        OnNotificationReceived?.Invoke(notification);
    }
}
