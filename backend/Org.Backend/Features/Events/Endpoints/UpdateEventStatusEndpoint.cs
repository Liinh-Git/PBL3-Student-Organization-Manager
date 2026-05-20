using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Events.Services;
using Org.Shared.Common;
using Org.Shared.Features.Events;

namespace Org.Backend.Features.Events.Endpoints;

public class UpdateEventStatusEndpoint : Endpoint<UpdateEventStatusRequest, ApiResponse<EventDto>>
{
    private readonly IEventService _eventService;

    public UpdateEventStatusEndpoint(IEventService eventService)
    {
        _eventService = eventService;
    }

    public override void Configure()
    {
        Put("/events/{id}/status");
        Description(b => b
            .Produces<ApiResponse<EventDto>>(200, "application/json")
            .Produces<ApiResponse<EventDto>>(400, "application/json")
            .Produces<ApiResponse<EventDto>>(401, "application/json")
            .Produces<ApiResponse<EventDto>>(403, "application/json")
            .Produces<ApiResponse<EventDto>>(404, "application/json")
            .WithTags("Events"));
    }

    public override async Task HandleAsync(UpdateEventStatusRequest req, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<EventDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var eventId = Route<Guid>("id");
            var evt = await _eventService.UpdateEventStatusAsync(eventId, userId, req, ct);

            Response = ApiResponse<EventDto>.SuccessResponse(evt, "Event status updated successfully");
        }
        catch (KeyNotFoundException)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<EventDto>.ErrorResponse("Event not found");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<EventDto>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<EventDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<EventDto>.ErrorResponse($"An error occurred: {ex.Message}");
        }
    }
}
