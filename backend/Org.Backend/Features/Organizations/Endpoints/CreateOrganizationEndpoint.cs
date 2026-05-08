using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Organizations.Services;
using Org.Backend.Features.Organizations.Validators;
using Org.Shared.Common;
using Org.Shared.Features.Organizations;

namespace Org.Backend.Features.Organizations.Endpoints;

/// <summary>
/// Endpoint for creating organization
/// POST /api/organizations
/// </summary>
public class CreateOrganizationEndpoint : Endpoint<CreateOrganizationRequest, ApiResponse<OrganizationDto>>
{
    private readonly IOrganizationService _organizationService;

    public CreateOrganizationEndpoint(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    public override void Configure()
    {
        Post("/organizations");
        Validator<CreateOrganizationRequestValidator>();
    }

    public override async Task HandleAsync(CreateOrganizationRequest req, CancellationToken ct)
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

            // Create organization
            var organization = await _organizationService.CreateOrganizationAsync(userId, req, ct);

            // Return success response
            Response = ApiResponse<OrganizationDto>.SuccessResponse(organization, "Organization created successfully");
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<OrganizationDto>.ErrorResponse(ex.Message);
        }
    }
}
