using FastEndpoints;
using Org.Backend.Features.Notifications.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;

namespace Org.Backend.Features.Notifications.Endpoints;

public class MarkAllNotificationsReadEndpoint : EndpointWithoutRequest<ApiResponse<bool>>
{
    private readonly INotificationService _notificationService;

    public MarkAllNotificationsReadEndpoint(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public override void Configure()
    {
        Post("/notifications/read-all");
        Description(b => b
            .Produces<ApiResponse<bool>>(200)
            .Produces<ApiResponse<object>>(400));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            await _notificationService.MarkAllNotificationsReadAsync(userId, ct);
            Response = ApiResponse<bool>.SuccessResponse(true, "All notifications marked as read");
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<bool>.ErrorResponse("Failed to mark all notifications as read", new List<string> { ex.Message });
        }
    }
}

