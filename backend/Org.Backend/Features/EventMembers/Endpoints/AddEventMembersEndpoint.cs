using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.EventMembers.Services;
using Org.Shared.Common;
using Org.Shared.Features.EventMembers;

namespace Org.Backend.Features.EventMembers.Endpoints;

public class AddEventMembersEndpoint : Endpoint<AddEventMembersRequest, ApiResponse<List<EventMemberDto>>>
{
    private readonly IEventMemberService _eventMemberService;

    public AddEventMembersEndpoint(IEventMemberService eventMemberService)
    {
        _eventMemberService = eventMemberService;
    }

    public override void Configure()
    {
        Post("/events/{eventId}/members");
    }

    public override async Task HandleAsync(AddEventMembersRequest req, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<List<EventMemberDto>>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var eventId = Route<Guid>("eventId");
            var result = await _eventMemberService.AddEventMembersAsync(eventId, userId, req, ct);
            Response = ApiResponse<List<EventMemberDto>>.SuccessResponse(result, "Event organizers updated");
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<List<EventMemberDto>>.ErrorResponse(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<List<EventMemberDto>>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<EventMemberDto>>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<List<EventMemberDto>>.ErrorResponse("Failed to add event organizers", new List<string> { ex.Message });
        }
    }
}

