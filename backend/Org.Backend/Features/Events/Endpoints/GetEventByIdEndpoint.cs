using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Events.Services;
using Org.Shared.Common;
using Org.Shared.Features.Events;

namespace Org.Backend.Features.Events.Endpoints;

/// <summary>
/// Endpoint for getting event by ID
/// GET /api/events/{id}
/// </summary>
public class GetEventByIdEndpoint : EndpointWithoutRequest<ApiResponse<EventDto>>
{
    private readonly IEventService _eventService;

    public GetEventByIdEndpoint(IEventService eventService)
    {
        _eventService = eventService;
    }

    public override void Configure()
    {
        Get("/events/{id}");
        Description(b => b
            .Produces<ApiResponse<EventDto>>(200, "application/json")
            .Produces<ApiResponse<EventDto>>(401, "application/json")
            .Produces<ApiResponse<EventDto>>(403, "application/json")
            .Produces<ApiResponse<EventDto>>(404, "application/json")
            .WithTags("Events"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<EventDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            // Get event ID from route
            var idStr = Route<string>("id");
            if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var eventId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<EventDto>.ErrorResponse("Invalid event ID");
                return;
            }

            var result = await _eventService.GetEventByIdAsync(eventId, userId, ct);
            Response = ApiResponse<EventDto>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<EventDto>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<EventDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<EventDto>.ErrorResponse("Failed to get event", new List<string> { ex.Message });
        }
    }
}
