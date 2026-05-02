// ---- SignalR Hub for real-time notification delivery ----
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Org.Backend.Hubs;

/// <summary>
/// SignalR hub for broadcasting notifications to connected clients in real-time.
/// Requires JWT authentication. User ID is automatically extracted from ClaimTypes.NameIdentifier.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        
        // Optional: Log connection for debugging
        var userId = Context.UserIdentifier;
        Console.WriteLine($"User {userId} connected to NotificationHub. ConnectionId: {Context.ConnectionId}");
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        
        // Optional: Log disconnection for debugging
        var userId = Context.UserIdentifier;
        if (exception is not null)
        {
            Console.WriteLine($"User {userId} disconnected from NotificationHub with error: {exception.Message}");
        }
        else
        {
            Console.WriteLine($"User {userId} disconnected from NotificationHub. ConnectionId: {Context.ConnectionId}");
        }
    }
}
