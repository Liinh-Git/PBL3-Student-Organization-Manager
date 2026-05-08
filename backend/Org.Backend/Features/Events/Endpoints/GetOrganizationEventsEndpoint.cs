using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Events.Services;
using Org.Shared.Common;
using Org.Shared.Features.Events;

namespace Org.Backend.Features.Events.Endpoints;

/// <summary>
/// Endpoint for getting organization events
/// GET /api/organizations/{orgId}/events
/// </summary>
public class GetOrganizationEventsEndpoint : EndpointWithoutRequest<ApiResponse<List<EventSummaryDto>>>
{
    private readonly IEventService _eventService;

    public GetOrganizationEventsEndpoint(IEventService eventService)
    {
        _eventService = eventService;
    }

    public override void Configure()
    {
        Get("/organizations/{orgId}/events");
        Description(b => b
            .Produces<ApiResponse<List<EventSummaryDto>>>(200, "application/json")
            .Produces<ApiResponse<List<EventSummaryDto>>>(401, "application/json")
            .Produces<ApiResponse<List<EventSummaryDto>>>(403, "application/json")
            .WithTags("Events"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<List<EventSummaryDto>>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            // Get orgId from route
            var orgIdStr = Route<string>("orgId");
            if (string.IsNullOrEmpty(orgIdStr) || !Guid.TryParse(orgIdStr, out var orgId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<List<EventSummaryDto>>.ErrorResponse("Invalid organization ID");
                return;
            }

            var result = await _eventService.GetOrganizationEventsAsync(orgId, userId, ct);
            Response = ApiResponse<List<EventSummaryDto>>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<List<EventSummaryDto>>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<EventSummaryDto>>.ErrorResponse("Failed to get events", new List<string> { ex.Message });
        }
    }
}
