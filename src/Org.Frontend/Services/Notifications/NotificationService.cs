using System.Net.Http.Json;
using Org.Frontend.Services.Auth;
using Org.Frontend.Services.SignalR;
using Org.Shared.Features.Notifications;

namespace Org.Frontend.Services.Notifications;

public sealed class NotificationService(
    IAuthenticatedBackendClient backendClient,
    ISignalRService signalRService) : INotificationService
{
    private readonly IAuthenticatedBackendClient _backendClient = backendClient;
    private readonly ISignalRService _signalRService = signalRService;
    private bool _realtimeStarted;

    public event Action<NotificationMessage>? OnNotificationReceived;

    public Task StartRealtimeAsync(CancellationToken ct = default)
    {
        if (_realtimeStarted)
            return Task.CompletedTask;

        _signalRService.OnNotificationReceived += HandleNotificationReceived;
        _realtimeStarted = true;
        return _signalRService.StartAsync(ct);
    }

    public async Task StopRealtimeAsync(CancellationToken ct = default)
    {
        if (!_realtimeStarted)
            return;

        _signalRService.OnNotificationReceived -= HandleNotificationReceived;
        _realtimeStarted = false;
        await _signalRService.StopAsync(ct);
    }

    public async Task<GetNotificationsResponse> GetNotificationsAsync(
        int page = 1,
        int pageSize = 20,
        bool? isRead = null,
        string? type = null,
        CancellationToken ct = default)
    {
        var url = $"api/notifications?page={page}&pageSize={pageSize}" +
            (isRead.HasValue ? $"&isRead={isRead.Value}" : "") +
            (!string.IsNullOrWhiteSpace(type) ? $"&type={type}" : "");

        var response = await _backendClient.GetFromJsonAsync<GetNotificationsResponse>(url, ct);
        return response ?? new GetNotificationsResponse([], 0, 0, page, pageSize);
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken ct = default)
    {
        var response = await _backendClient.GetFromJsonAsync<GetUnreadCountResponse>(
            "api/notifications/unread-count",
            ct);

        return response?.Count ?? 0;
    }

    public async Task<NotificationDto> GetNotificationByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _backendClient.GetFromJsonAsync<GetNotificationByIdResponse>(
            $"api/notifications/{id}",
            ct);

        return response?.Data ?? throw new InvalidOperationException("Notification not found");
    }

    public async Task<NotificationDto> MarkAsReadAsync(Guid id, CancellationToken ct = default)
    {
        var result = await _backendClient.PutAsJsonAsync<object, MarkAsReadResponse>(
            $"api/notifications/{id}/read",
            new { },
            ct);

        return result?.Data ?? throw new InvalidOperationException("Failed to mark notification as read");
    }

    public async Task<int> MarkAllAsReadAsync(CancellationToken ct = default)
    {
        var result = await _backendClient.PutAsJsonAsync<object, MarkAllAsReadResponse>(
            "api/notifications/read-all",
            new { },
            ct);

        return result?.UpdatedCount ?? 0;
    }

    public async Task<int> DeleteNotificationAsync(Guid id, CancellationToken ct = default)
    {
        await _backendClient.DeleteAsync($"api/notifications/{id}", ct);
        return 1;
    }

    public async Task<int> ClearAllNotificationsAsync(bool onlyRead = false, CancellationToken ct = default)
    {
        var request = new ClearNotificationsRequest(onlyRead);
        using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "api/notifications/clear-all")
        {
            Content = JsonContent.Create(request)
        };

        using var response = await _backendClient.SendAsync(requestMessage, ct);
        var result = await response.Content.ReadFromJsonAsync<ClearNotificationsResponse>(ct);
        return result?.DeletedCount ?? 0;
    }

    public void HandleNotificationReceived(NotificationMessage notification)
    {
        OnNotificationReceived?.Invoke(notification);
    }
}
