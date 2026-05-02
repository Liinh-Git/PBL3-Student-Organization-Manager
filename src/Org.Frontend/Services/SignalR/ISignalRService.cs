// ---- Service interface cho SignalR client ----
using Org.Shared.Features.Notifications;

namespace Org.Frontend.Services.SignalR;

public interface ISignalRService
{
    event Action<NotificationMessage>? OnNotificationReceived;
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
    bool IsConnected { get; }
}
