using FastEndpoints;
using Org.Backend.Features.Notifications.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Notifications;

namespace Org.Backend.Features.Notifications.Endpoints;

public class GetUnreadCountEndpoint : EndpointWithoutRequest<ApiResponse<UnreadCountDto>>
{
    private readonly INotificationService _notificationService;

    public GetUnreadCountEndpoint(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public override void Configure()
    {
        Get("/notifications/unread-count");
        Description(b => b
            .Produces<ApiResponse<UnreadCountDto>>(200)
            .Produces<ApiResponse<object>>(400));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _notificationService.GetUnreadCountAsync(userId, ct);
            Response = ApiResponse<UnreadCountDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<UnreadCountDto>.ErrorResponse("Failed to get unread count", new List<string> { ex.Message });
        }
    }
}

