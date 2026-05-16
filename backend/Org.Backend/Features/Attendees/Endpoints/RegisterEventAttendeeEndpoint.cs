using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Attendees.Services;
using Org.Shared.Common;
using Org.Shared.Features.Attendees;

namespace Org.Backend.Features.Attendees.Endpoints;

public class RegisterEventAttendeeEndpoint : Endpoint<RegisterEventAttendeeRequest, ApiResponse<AttendeeDto>>
{
    private readonly IAttendeeService _attendeeService;

    public RegisterEventAttendeeEndpoint(IAttendeeService attendeeService)
    {
        _attendeeService = attendeeService;
    }

    public override void Configure()
    {
        Post("/events/{id}/attendees/register");
        Description(b => b
            .Produces<ApiResponse<AttendeeDto>>(200, "application/json")
            .Produces<ApiResponse<AttendeeDto>>(401, "application/json")
            .Produces<ApiResponse<AttendeeDto>>(403, "application/json")
            .Produces<ApiResponse<AttendeeDto>>(404, "application/json")
            .WithTags("Attendees"));
    }

    public override async Task HandleAsync(RegisterEventAttendeeRequest req, CancellationToken ct)
    {
        if (!TryGetRouteEventId(out var eventId) || !TryGetUserId(out var userId))
        {
            return;
        }

        try
        {
            var result = await _attendeeService.RegisterAsync(eventId, userId, req, ct);
            Response = ApiResponse<AttendeeDto>.SuccessResponse(result, "Registered successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<AttendeeDto>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<AttendeeDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<AttendeeDto>.ErrorResponse("Failed to register", new List<string> { ex.Message });
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
        Response = ApiResponse<AttendeeDto>.ErrorResponse("Invalid event ID");
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
        Response = ApiResponse<AttendeeDto>.ErrorResponse("Invalid or missing user ID in token");
        return false;
    }
}
