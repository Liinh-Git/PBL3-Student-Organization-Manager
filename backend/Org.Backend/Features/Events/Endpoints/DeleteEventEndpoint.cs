using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Events.Services;
using Org.Shared.Common;

namespace Org.Backend.Features.Events.Endpoints;

public class DeleteEventEndpoint : EndpointWithoutRequest<ApiResponse<bool>>
{
    private readonly IEventService _eventService;

    public DeleteEventEndpoint(IEventService eventService)
    {
        _eventService = eventService;
    }

    public override void Configure()
    {
        Delete("/events/{id}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<bool>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var eventId = Route<Guid>("id");

            var result = await _eventService.DeleteEventAsync(eventId, userId, ct);

            Response = ApiResponse<bool>.SuccessResponse(result, "Event deleted successfully");
        }
        catch (KeyNotFoundException)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<bool>.ErrorResponse("Event not found");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}");
        }
    }
}
