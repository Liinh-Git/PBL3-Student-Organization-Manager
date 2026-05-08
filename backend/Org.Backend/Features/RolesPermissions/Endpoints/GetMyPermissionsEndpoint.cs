using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.RolesPermissions.Services;
using Org.Shared.Common;
using Org.Shared.Features.RolesPermissions;

namespace Org.Backend.Features.RolesPermissions.Endpoints;

/// <summary>
/// Request for getting my permissions
/// </summary>
public class GetMyPermissionsRequest
{
    public Guid OrgId { get; set; }
}

/// <summary>
/// Endpoint for getting current user's permissions in organization
/// GET /api/organizations/{orgId}/permissions/me
/// </summary>
public class GetMyPermissionsEndpoint : Endpoint<GetMyPermissionsRequest, ApiResponse<MyPermissionsResponse>>
{
    private readonly IRoleService _roleService;

    public GetMyPermissionsEndpoint(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public override void Configure()
    {
        Get("/organizations/{orgId}/permissions/me");
        Description(b => b
            .Produces<ApiResponse<MyPermissionsResponse>>(200, "application/json")
            .Produces<ApiResponse<MyPermissionsResponse>>(401, "application/json")
            .Produces<ApiResponse<MyPermissionsResponse>>(403, "application/json")
            .WithTags("RolesPermissions"));
    }

    public override async Task HandleAsync(GetMyPermissionsRequest req, CancellationToken ct)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<MyPermissionsResponse>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var result = await _roleService.GetMyPermissionsAsync(req.OrgId, userId, ct);
            Response = ApiResponse<MyPermissionsResponse>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<MyPermissionsResponse>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<MyPermissionsResponse>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<MyPermissionsResponse>.ErrorResponse("Failed to get permissions", new List<string> { ex.Message });
        }
    }
}
