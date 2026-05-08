using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Organizations.Services;
using Org.Shared.Common;

namespace Org.Backend.Features.Organizations.Endpoints;

/// <summary>
/// Endpoint for deleting organization
/// DELETE /api/organizations/{id}
/// </summary>
public class DeleteOrganizationEndpoint : EndpointWithoutRequest<ApiResponse<bool>>
{
    private readonly IOrganizationService _organizationService;

    public DeleteOrganizationEndpoint(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    public override void Configure()
    {
        Delete("/organizations/{id}");
        Description(b => b
            .Produces<ApiResponse<bool>>(200, "application/json")
            .Produces<ApiResponse<bool>>(401, "application/json")
            .Produces<ApiResponse<bool>>(403, "application/json")
            .Produces<ApiResponse<bool>>(404, "application/json")
            .WithTags("Organizations"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<bool>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            // Get organization ID from route
            var orgIdStr = Route<string>("id");
            if (string.IsNullOrEmpty(orgIdStr) || !Guid.TryParse(orgIdStr, out var orgId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<bool>.ErrorResponse("Invalid organization ID");
                return;
            }

            // Delete organization
            await _organizationService.DeleteOrganizationAsync(orgId, userId, ct);

            // Return success response
            Response = ApiResponse<bool>.SuccessResponse(true, "Organization deleted successfully");
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
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }
}
