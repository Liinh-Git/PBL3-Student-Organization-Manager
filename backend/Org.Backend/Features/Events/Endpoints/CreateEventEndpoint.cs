using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Events.Services;
using Org.Backend.Features.Events.Validators;
using Org.Shared.Common;
using Org.Shared.Features.Events;

namespace Org.Backend.Features.Events.Endpoints;

public class CreateEventEndpoint : Endpoint<CreateEventRequest, ApiResponse<EventDto>>
{
    private readonly IEventService _eventService;

    public CreateEventEndpoint(IEventService eventService)
    {
        _eventService = eventService;
    }

    public override void Configure()
    {
        Post("/organizations/{orgId}/events");
        Validator<CreateEventRequestValidator>();
    }

    public override async Task HandleAsync(CreateEventRequest req, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<EventDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var orgId = Route<Guid>("orgId");

            var evt = await _eventService.CreateEventAsync(orgId, userId, req, ct);

            Response = ApiResponse<EventDto>.SuccessResponse(evt, "Event created successfully");
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
