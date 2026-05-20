using FastEndpoints;
using Org.Backend.Features.Events.Services;
using Org.Shared.Common;
using Org.Shared.Features.Events;

namespace Org.Backend.Features.Events.Endpoints;

/// <summary>
/// Endpoint for getting public event by ID (no auth required)
/// GET /api/events/{id}/public
/// </summary>
public class GetPublicEventByIdEndpoint : EndpointWithoutRequest<ApiResponse<EventPublicDto>>
{
    private readonly IEventService _eventService;

    public GetPublicEventByIdEndpoint(IEventService eventService)
    {
        _eventService = eventService;
    }

    public override void Configure()
    {
        Get("/events/{id}/public");
        AllowAnonymous();
        Description(b => b
            .Produces<ApiResponse<EventPublicDto>>(200, "application/json")
            .Produces<ApiResponse<EventPublicDto>>(403, "application/json")
            .Produces<ApiResponse<EventPublicDto>>(404, "application/json")
            .WithTags("Events"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            // Get event ID from route
            var idStr = Route<string>("id");
            if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var eventId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<EventPublicDto>.ErrorResponse("Invalid event ID");
                return;
            }

            var result = await _eventService.GetPublicEventByIdAsync(eventId, ct);
            Response = ApiResponse<EventPublicDto>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<EventPublicDto>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<EventPublicDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<EventPublicDto>.ErrorResponse("Failed to get public event", new List<string> { ex.Message });
        }
    }
}
