using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Organizations.Services;
using Org.Shared.Common;
using Org.Shared.Features.Organizations;

namespace Org.Backend.Features.Organizations.Endpoints;

/// <summary>
/// Endpoint for getting default organization for current user
/// GET /api/organizations/default
/// </summary>
public class GetDefaultOrganizationEndpoint : EndpointWithoutRequest<ApiResponse<OrganizationDto>>
{
    private readonly IOrganizationService _organizationService;

    public GetDefaultOrganizationEndpoint(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    public override void Configure()
    {
        Get("/organizations/default");
        Description(b => b
            .Produces<ApiResponse<OrganizationDto>>(200, "application/json")
            .Produces<ApiResponse<OrganizationDto>>(401, "application/json")
            .Produces<ApiResponse<OrganizationDto>>(404, "application/json")
            .WithTags("Organizations"));
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
                Response = ApiResponse<OrganizationDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var result = await _organizationService.GetDefaultOrganizationAsync(userId, ct);
            Response = ApiResponse<OrganizationDto>.SuccessResponse(result);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<OrganizationDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<OrganizationDto>.ErrorResponse("Failed to get default organization", new List<string> { ex.Message });
        }
    }
}
