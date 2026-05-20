using FastEndpoints;
using Org.Backend.Features.Notifications.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Notifications;

namespace Org.Backend.Features.Notifications.Endpoints;

public class GetNotificationsEndpoint : EndpointWithoutRequest<ApiResponse<List<NotificationDto>>>
{
    private readonly INotificationService _notificationService;

    public GetNotificationsEndpoint(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public override void Configure()
    {
        Get("/notifications");
        Description(b => b
            .Produces<ApiResponse<List<NotificationDto>>>(200)
            .Produces<ApiResponse<object>>(400));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _notificationService.GetNotificationsAsync(userId, ct);
            Response = ApiResponse<List<NotificationDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<NotificationDto>>.ErrorResponse("Failed to get notifications", new List<string> { ex.Message });
        }
    }
}

