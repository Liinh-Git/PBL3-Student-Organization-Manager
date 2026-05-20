using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Organizations.Services;
using Org.Backend.Features.Organizations.Validators;
using Org.Shared.Common;
using Org.Shared.Features.Organizations;

namespace Org.Backend.Features.Organizations.Endpoints;

/// <summary>
/// Endpoint for updating organization
/// PUT /api/organizations/{id}
/// </summary>
public class UpdateOrganizationEndpoint : Endpoint<UpdateOrganizationRequest, ApiResponse<OrganizationDto>>
{
    private readonly IOrganizationService _organizationService;

    public UpdateOrganizationEndpoint(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    public override void Configure()
    {
        Put("/organizations/{id}");
        Validator<UpdateOrganizationRequestValidator>();
    }

    public override async Task HandleAsync(UpdateOrganizationRequest req, CancellationToken ct)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<OrganizationDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            // Get organization ID from route
            var orgIdStr = Route<string>("id");
            if (string.IsNullOrEmpty(orgIdStr) || !Guid.TryParse(orgIdStr, out var orgId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<OrganizationDto>.ErrorResponse("Invalid organization ID");
                return;
            }

            // Update organization
            var organization = await _organizationService.UpdateOrganizationAsync(orgId, userId, req, ct);

            // Return success response
            Response = ApiResponse<OrganizationDto>.SuccessResponse(organization, "Organization updated successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<OrganizationDto>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<OrganizationDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<OrganizationDto>.ErrorResponse(ex.Message);
        }
    }
}
