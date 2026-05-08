using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Milestones.Services;
using Org.Shared.Common;
using Org.Shared.Features.Milestones;

namespace Org.Backend.Features.Milestones.Endpoints;

public class UpdateMilestoneEndpoint : Endpoint<UpdateMilestoneRequest, ApiResponse<MilestoneDto>>
{
    private readonly IMilestoneService _milestoneService;

    public UpdateMilestoneEndpoint(IMilestoneService milestoneService)
    {
        _milestoneService = milestoneService;
    }

    public override void Configure()
    {
        Put("/milestones/{id}");
        Description(b => b.WithTags("Milestones"));
    }

    public override async Task HandleAsync(UpdateMilestoneRequest req, CancellationToken ct)
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

            var idStr = Route<string>("id");
            if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var milestoneId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<MilestoneDto>.ErrorResponse("Invalid milestone ID");
                return;
            }

            var result = await _milestoneService.UpdateMilestoneAsync(milestoneId, req, userId, ct);
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
            Response = ApiResponse<MilestoneDto>.ErrorResponse("Failed to update milestone", new List<string> { ex.Message });
        }
    }
}
