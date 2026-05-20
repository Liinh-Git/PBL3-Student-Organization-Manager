using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Milestones.Services;
using Org.Shared.Common;
using Org.Shared.Features.Milestones;

namespace Org.Backend.Features.Milestones.Endpoints;

public class CreateMilestoneEndpoint : Endpoint<CreateMilestoneRequest, ApiResponse<MilestoneDto>>
{
    private readonly IMilestoneService _milestoneService;

    public CreateMilestoneEndpoint(IMilestoneService milestoneService)
    {
        _milestoneService = milestoneService;
    }

    public override void Configure()
    {
        Post("/events/{eventId}/milestones");
        Description(b => b
            .Produces<ApiResponse<MilestoneDto>>(200, "application/json")
            .Produces<ApiResponse<MilestoneDto>>(400, "application/json")
            .Produces<ApiResponse<MilestoneDto>>(401, "application/json")
            .Produces<ApiResponse<MilestoneDto>>(403, "application/json")
            .WithTags("Milestones"));
    }

    public override async Task HandleAsync(CreateMilestoneRequest req, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<MilestoneDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var eventIdStr = Route<string>("eventId");
            if (string.IsNullOrEmpty(eventIdStr) || !Guid.TryParse(eventIdStr, out var eventId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<MilestoneDto>.ErrorResponse("Invalid event ID");
                return;
            }

            var result = await _milestoneService.CreateMilestoneAsync(eventId, req, userId, ct);
            Response = ApiResponse<MilestoneDto>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<MilestoneDto>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<MilestoneDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<MilestoneDto>.ErrorResponse("Failed to create milestone", new List<string> { ex.Message });
        }
    }
}
