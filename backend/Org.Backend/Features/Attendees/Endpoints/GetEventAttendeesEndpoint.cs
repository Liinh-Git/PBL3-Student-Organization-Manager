using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Attendees.Services;
using Org.Shared.Common;
using Org.Shared.Features.Attendees;

namespace Org.Backend.Features.Attendees.Endpoints;

public class GetEventAttendeesEndpoint : EndpointWithoutRequest<ApiResponse<List<AttendeeDto>>>
{
    private readonly IAttendeeService _attendeeService;

    public GetEventAttendeesEndpoint(IAttendeeService attendeeService)
    {
        _attendeeService = attendeeService;
    }

    public override void Configure()
    {
        Get("/events/{id}/attendees");
        Description(b => b
            .Produces<ApiResponse<List<AttendeeDto>>>(200, "application/json")
            .Produces<ApiResponse<List<AttendeeDto>>>(401, "application/json")
            .Produces<ApiResponse<List<AttendeeDto>>>(403, "application/json")
            .Produces<ApiResponse<List<AttendeeDto>>>(404, "application/json")
            .WithTags("Attendees"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!TryGetRouteEventId(out var eventId) || !TryGetUserId(out var userId))
        {
            return;
        }

        try
        {
            var result = await _attendeeService.GetEventAttendeesAsync(eventId, userId, ct);
            Response = ApiResponse<List<AttendeeDto>>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<List<AttendeeDto>>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<List<AttendeeDto>>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<AttendeeDto>>.ErrorResponse("Failed to get attendees", new List<string> { ex.Message });
        }
    }

    private bool TryGetRouteEventId(out Guid eventId)
    {
        eventId = Guid.Empty;
        var idStr = Route<string>("id");
        if (!string.IsNullOrEmpty(idStr) && Guid.TryParse(idStr, out eventId))
        {
            return true;
        }

        HttpContext.Response.StatusCode = 400;
        Response = ApiResponse<List<AttendeeDto>>.ErrorResponse("Invalid event ID");
        return false;
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out userId))
        {
            return true;
        }

        HttpContext.Response.StatusCode = 401;
        Response = ApiResponse<List<AttendeeDto>>.ErrorResponse("Invalid or missing user ID in token");
        return false;
    }
}
