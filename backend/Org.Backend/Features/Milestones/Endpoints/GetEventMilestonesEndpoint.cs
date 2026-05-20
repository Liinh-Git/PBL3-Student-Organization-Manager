using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Milestones.Services;
using Org.Shared.Common;
using Org.Shared.Features.Milestones;

namespace Org.Backend.Features.Milestones.Endpoints;

/// <summary>
/// Endpoint for getting event milestones
/// GET /api/events/{eventId}/milestones
/// </summary>
public class GetEventMilestonesEndpoint : EndpointWithoutRequest<ApiResponse<List<MilestoneDto>>>
{
    private readonly IMilestoneService _milestoneService;

    public GetEventMilestonesEndpoint(IMilestoneService milestoneService)
    {
        _milestoneService = milestoneService;
    }

    public override void Configure()
    {
        Get("/events/{eventId}/milestones");
        Description(b => b
            .Produces<ApiResponse<List<MilestoneDto>>>(200, "application/json")
            .Produces<ApiResponse<List<MilestoneDto>>>(401, "application/json")
            .Produces<ApiResponse<List<MilestoneDto>>>(403, "application/json")
            .Produces<ApiResponse<List<MilestoneDto>>>(404, "application/json")
            .WithTags("Milestones"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<List<MilestoneDto>>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var eventIdStr = Route<string>("eventId");
            if (string.IsNullOrEmpty(eventIdStr) || !Guid.TryParse(eventIdStr, out var eventId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<List<MilestoneDto>>.ErrorResponse("Invalid event ID");
                return;
            }

            var result = await _milestoneService.GetEventMilestonesAsync(eventId, userId, ct);
            Response = ApiResponse<List<MilestoneDto>>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<List<MilestoneDto>>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<List<MilestoneDto>>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<MilestoneDto>>.ErrorResponse("Failed to get milestones", new List<string> { ex.Message });
        }
    }
}
