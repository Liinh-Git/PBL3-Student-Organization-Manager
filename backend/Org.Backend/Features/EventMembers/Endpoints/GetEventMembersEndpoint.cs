using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.EventMembers.Services;
using Org.Shared.Common;
using Org.Shared.Features.EventMembers;

namespace Org.Backend.Features.EventMembers.Endpoints;

public class GetEventMembersEndpoint : EndpointWithoutRequest<ApiResponse<List<EventMemberDto>>>
{
    private readonly IEventMemberService _eventMemberService;

    public GetEventMembersEndpoint(IEventMemberService eventMemberService)
    {
        _eventMemberService = eventMemberService;
    }

    public override void Configure()
    {
        Get("/events/{eventId}/members");
    }

    public override async Task HandleAsync(CancellationToken ct)
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
            var result = await _eventMemberService.GetEventMembersAsync(eventId, userId, ct);
            Response = ApiResponse<List<EventMemberDto>>.SuccessResponse(result);
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
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<List<EventMemberDto>>.ErrorResponse("Failed to get event members", new List<string> { ex.Message });
        }
    }
}

