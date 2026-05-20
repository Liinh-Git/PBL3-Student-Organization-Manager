using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.RolesPermissions.Services;
using Org.Shared.Common;
using Org.Shared.Features.RolesPermissions;

namespace Org.Backend.Features.RolesPermissions.Endpoints;

/// <summary>
/// Request for getting organization roles
/// </summary>
public class GetOrganizationRolesRequest
{
    public Guid OrgId { get; set; }
}

/// <summary>
/// Endpoint for getting organization roles
/// GET /api/organizations/{orgId}/roles
/// </summary>
public class GetOrganizationRolesEndpoint : Endpoint<GetOrganizationRolesRequest, ApiResponse<List<RoleDto>>>
{
    private readonly IRoleService _roleService;

    public GetOrganizationRolesEndpoint(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public override void Configure()
    {
        Get("/organizations/{orgId}/roles");
        Description(b => b
            .Produces<ApiResponse<List<RoleDto>>>(200, "application/json")
            .Produces<ApiResponse<List<RoleDto>>>(401, "application/json")
            .Produces<ApiResponse<List<RoleDto>>>(403, "application/json")
            .WithTags("RolesPermissions"));
    }

    public override async Task HandleAsync(GetOrganizationRolesRequest req, CancellationToken ct)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<List<RoleDto>>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var result = await _roleService.GetOrganizationRolesAsync(req.OrgId, userId, ct);
            Response = ApiResponse<List<RoleDto>>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<List<RoleDto>>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<RoleDto>>.ErrorResponse("Failed to get roles", new List<string> { ex.Message });
        }
    }
}
