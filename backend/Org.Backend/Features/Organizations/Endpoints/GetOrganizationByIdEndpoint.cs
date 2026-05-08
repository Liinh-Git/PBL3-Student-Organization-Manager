using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Organizations.Services;
using Org.Shared.Common;
using Org.Shared.Features.Organizations;

namespace Org.Backend.Features.Organizations.Endpoints;

/// <summary>
/// Request for getting organization by ID
/// </summary>
public class GetOrganizationByIdRequest
{
    public Guid Id { get; set; }
}

/// <summary>
/// Endpoint for getting organization by ID
/// GET /api/organizations/{id}
/// </summary>
public class GetOrganizationByIdEndpoint : Endpoint<GetOrganizationByIdRequest, ApiResponse<OrganizationDto>>
{
    private readonly IOrganizationService _organizationService;

    public GetOrganizationByIdEndpoint(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    public override void Configure()
    {
        Get("/organizations/{id}");
        Description(b => b
            .Produces<ApiResponse<OrganizationDto>>(200, "application/json")
            .Produces<ApiResponse<OrganizationDto>>(401, "application/json")
            .Produces<ApiResponse<OrganizationDto>>(403, "application/json")
            .Produces<ApiResponse<OrganizationDto>>(404, "application/json")
            .WithTags("Organizations"));
    }

    public override async Task HandleAsync(GetOrganizationByIdRequest req, CancellationToken ct)
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

            var result = await _organizationService.GetOrganizationByIdAsync(req.Id, userId, ct);
            Response = ApiResponse<OrganizationDto>.SuccessResponse(result);
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
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<OrganizationDto>.ErrorResponse("Failed to get organization", new List<string> { ex.Message });
        }
    }
}
