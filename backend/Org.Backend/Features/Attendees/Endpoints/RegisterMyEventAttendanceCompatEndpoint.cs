using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Attendees.Services;
using Org.Shared.Common;
using Org.Shared.Features.Attendees;

namespace Org.Backend.Features.Attendees.Endpoints;

/// <summary>
/// POST /api/events/{eventId}/attendees
/// Compatibility route for clients that call the generic attendees path.
/// </summary>
public class RegisterMyEventAttendanceCompatEndpoint : EndpointWithoutRequest<ApiResponse<AttendeeRegistrationDto>>
{
    private readonly IAttendeeService _attendeeService;

    public RegisterMyEventAttendanceCompatEndpoint(IAttendeeService attendeeService)
    {
        _attendeeService = attendeeService;
    }

    public override void Configure()
    {
        Post("/events/{eventId}/attendees");
        Description(b => b
            .Produces<ApiResponse<AttendeeRegistrationDto>>(200, "application/json")
            .Produces<ApiResponse<AttendeeRegistrationDto>>(401, "application/json")
            .Produces<ApiResponse<AttendeeRegistrationDto>>(403, "application/json")
            .Produces<ApiResponse<AttendeeRegistrationDto>>(404, "application/json")
            .WithTags("Attendees"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<AttendeeRegistrationDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var eventId = Route<Guid>("eventId");
            var result = await _attendeeService.RegisterMeAsync(eventId, userId, ct);
            Response = ApiResponse<AttendeeRegistrationDto>.SuccessResponse(result, "Joined event successfully");
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<AttendeeRegistrationDto>.ErrorResponse(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<AttendeeRegistrationDto>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<AttendeeRegistrationDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<AttendeeRegistrationDto>.ErrorResponse("Failed to join event", new List<string> { ex.Message });
        }
    }
}
