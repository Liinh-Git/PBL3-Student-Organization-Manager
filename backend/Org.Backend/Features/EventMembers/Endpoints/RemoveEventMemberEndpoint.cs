using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.EventMembers.Services;
using Org.Shared.Common;

namespace Org.Backend.Features.EventMembers.Endpoints;

public class RemoveEventMemberEndpoint : EndpointWithoutRequest<ApiResponse<bool>>
{
    private readonly IEventMemberService _eventMemberService;

    public RemoveEventMemberEndpoint(IEventMemberService eventMemberService)
    {
        _eventMemberService = eventMemberService;
    }

    public override void Configure()
    {
        Delete("/event-members/{id}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<bool>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var eventMemberId = Route<Guid>("id");
            var result = await _eventMemberService.RemoveEventMemberAsync(eventMemberId, userId, ct);
            Response = ApiResponse<bool>.SuccessResponse(result, "Event organizer removed");
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<bool>.ErrorResponse("Failed to remove event organizer", new List<string> { ex.Message });
        }
    }
}

