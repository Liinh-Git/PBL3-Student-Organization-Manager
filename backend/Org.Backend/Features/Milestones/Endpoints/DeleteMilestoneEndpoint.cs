using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Milestones.Services;
using Org.Shared.Common;

namespace Org.Backend.Features.Milestones.Endpoints;

public class DeleteMilestoneEndpoint : EndpointWithoutRequest<ApiResponse<bool>>
{
    private readonly IMilestoneService _milestoneService;

    public DeleteMilestoneEndpoint(IMilestoneService milestoneService)
    {
        _milestoneService = milestoneService;
    }

    public override void Configure()
    {
        Delete("/milestones/{id}");
        Description(b => b.WithTags("Milestones"));
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
                Response = ApiResponse<bool>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var idStr = Route<string>("id");
            if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var milestoneId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<bool>.ErrorResponse("Invalid milestone ID");
                return;
            }

            await _milestoneService.DeleteMilestoneAsync(milestoneId, userId, ct);
            Response = ApiResponse<bool>.SuccessResponse(true, "Milestone deleted successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<bool>.ErrorResponse("Failed to delete milestone", new List<string> { ex.Message });
        }
    }
}
