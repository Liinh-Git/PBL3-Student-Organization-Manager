using FastEndpoints;
using Org.Backend.Features.Attendees.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Attendees;

namespace Org.Backend.Features.Attendees.Endpoints;

/// <summary>
/// POST /api/events/{eventId}/attendees/me/check-in-request
/// </summary>
public class RequestMyCheckInEndpoint : Endpoint<RequestCheckInRequest, ApiResponse<AttendeeRegistrationDto>>
{
    private readonly IAttendeeService _attendeeService;

    public RequestMyCheckInEndpoint(IAttendeeService attendeeService)
    {
        _attendeeService = attendeeService;
    }

    public override void Configure()
    {
        Post("/events/{eventId}/attendees/me/check-in-request");
        Description(b => b
            .Produces<ApiResponse<AttendeeRegistrationDto>>(200, "application/json")
            .Produces<ApiResponse<AttendeeRegistrationDto>>(400, "application/json")
            .Produces<ApiResponse<AttendeeRegistrationDto>>(401, "application/json")
            .Produces<ApiResponse<AttendeeRegistrationDto>>(404, "application/json")
            .WithTags("Attendees"));
    }

    public override async Task HandleAsync(RequestCheckInRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var eventId = Route<Guid>("eventId");
            var result = await _attendeeService.RequestMyCheckInAsync(eventId, userId, req, ct);
            Response = ApiResponse<AttendeeRegistrationDto>.SuccessResponse(result, "Check-in request submitted");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 401;
            Response = ApiResponse<AttendeeRegistrationDto>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<AttendeeRegistrationDto>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<AttendeeRegistrationDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<AttendeeRegistrationDto>.ErrorResponse("Failed to request check-in", new List<string> { ex.Message });
        }
    }
}

