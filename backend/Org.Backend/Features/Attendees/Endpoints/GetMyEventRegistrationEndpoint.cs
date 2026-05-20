using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Attendees.Services;
using Org.Shared.Common;
using Org.Shared.Features.Attendees;

namespace Org.Backend.Features.Attendees.Endpoints;

/// <summary>
/// GET /api/events/{eventId}/attendees/me
/// </summary>
public class GetMyEventRegistrationEndpoint : EndpointWithoutRequest<ApiResponse<AttendeeRegistrationDto>>
{
    private readonly IAttendeeService _attendeeService;

    public GetMyEventRegistrationEndpoint(IAttendeeService attendeeService)
    {
        _attendeeService = attendeeService;
    }

    public override void Configure()
    {
        Get("/events/{eventId}/attendees/me");
        Description(b => b
            .Produces<ApiResponse<AttendeeRegistrationDto>>(200, "application/json")
            .Produces<ApiResponse<AttendeeRegistrationDto>>(401, "application/json")
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
            var result = await _attendeeService.GetMyRegistrationAsync(eventId, userId, ct);
            Response = ApiResponse<AttendeeRegistrationDto>.SuccessResponse(result);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<AttendeeRegistrationDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<AttendeeRegistrationDto>.ErrorResponse("Failed to get registration", new List<string> { ex.Message });
        }
    }
}
