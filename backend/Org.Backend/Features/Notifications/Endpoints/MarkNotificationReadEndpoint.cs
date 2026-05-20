using FastEndpoints;
using Org.Backend.Features.Notifications.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Notifications;

namespace Org.Backend.Features.Notifications.Endpoints;

public class MarkNotificationReadEndpoint : Endpoint<MarkNotificationReadRequest, ApiResponse<NotificationDto>>
{
    private readonly INotificationService _notificationService;

    public MarkNotificationReadEndpoint(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public override void Configure()
    {
        Post("/notifications/{id}/read");
        Description(b => b
            .Produces<ApiResponse<NotificationDto>>(200)
            .Produces<ApiResponse<object>>(403)
            .Produces<ApiResponse<object>>(404));
    }

    public override async Task HandleAsync(MarkNotificationReadRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _notificationService.MarkNotificationReadAsync(userId, req.Id, ct);
            Response = ApiResponse<NotificationDto>.SuccessResponse(result, "Notification marked as read");
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<NotificationDto>.ErrorResponse(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<NotificationDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<NotificationDto>.ErrorResponse("Failed to mark notification as read", new List<string> { ex.Message });
        }
    }
}

public record MarkNotificationReadRequest
{
    public Guid Id { get; init; }
}

