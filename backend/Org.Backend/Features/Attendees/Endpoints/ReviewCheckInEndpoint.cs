using FastEndpoints;
using Org.Backend.Features.Attendees.Services;
using Org.Backend.Infrastructure.Auth;
using Org.Shared.Common;
using Org.Shared.Features.Attendees;

namespace Org.Backend.Features.Attendees.Endpoints;

/// <summary>
/// PUT /api/attendees/{attendeeId}/check-in-review
/// </summary>
public class ReviewCheckInEndpoint : Endpoint<ReviewCheckInRequest, ApiResponse<AttendeeDto>>
{
    private readonly IAttendeeService _attendeeService;

    public ReviewCheckInEndpoint(IAttendeeService attendeeService)
    {
        _attendeeService = attendeeService;
    }

    public override void Configure()
    {
        Put("/attendees/{attendeeId}/check-in-review");
        Description(b => b
            .Produces<ApiResponse<AttendeeDto>>(200, "application/json")
            .Produces<ApiResponse<AttendeeDto>>(400, "application/json")
            .Produces<ApiResponse<AttendeeDto>>(401, "application/json")
            .Produces<ApiResponse<AttendeeDto>>(403, "application/json")
            .Produces<ApiResponse<AttendeeDto>>(404, "application/json")
            .WithTags("Attendees"));
    }

    public override async Task HandleAsync(ReviewCheckInRequest req, CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();
            var attendeeId = Route<Guid>("attendeeId");
            var result = await _attendeeService.ReviewCheckInAsync(attendeeId, userId, req, ct);
            Response = ApiResponse<AttendeeDto>.SuccessResponse(
                result,
                req.Approve ? "Check-in approved" : "Check-in rejected");
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
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<AttendeeDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<AttendeeDto>.ErrorResponse("Failed to review check-in", new List<string> { ex.Message });
        }
    }
}

