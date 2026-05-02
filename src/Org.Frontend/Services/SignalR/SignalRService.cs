// ---- Service implementation cho SignalR client ----
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Org.Frontend.Services.Auth;
using Org.Shared.Features.Notifications;

namespace Org.Frontend.Services.SignalR;

public sealed class SignalRService(
    IAccessTokenStore accessTokenStore,
    NavigationManager navigation,
    IConfiguration configuration) : ISignalRService, IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly NavigationManager _navigation = navigation;
    private readonly IAccessTokenStore _accessTokenStore = accessTokenStore;
    private readonly string? _backendApiBaseUrl = configuration["BackendApi:BaseUrl"];

    public event Action<NotificationMessage>? OnNotificationReceived;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_connection?.State == HubConnectionState.Connected)
            return;

        var token = _accessTokenStore.AccessToken;
        if (string.IsNullOrEmpty(token))
            return;

        var hubUrl = ResolveHubUrl();
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<NotificationMessage>("ReceiveNotification", (notification) =>
        {
            OnNotificationReceived?.Invoke(notification);
        });

        try
        {
            await _connection.StartAsync(ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR connection failed: {ex.Message}");
        }
    }

    private string ResolveHubUrl()
    {
        if (!string.IsNullOrWhiteSpace(_backendApiBaseUrl))
            return $"{_backendApiBaseUrl.TrimEnd('/')}/hubs/notifications";

        return _navigation.ToAbsoluteUri("/hubs/notifications").ToString();
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (_connection != null)
        {
            await _connection.StopAsync(ct);
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
        }
    }
}
