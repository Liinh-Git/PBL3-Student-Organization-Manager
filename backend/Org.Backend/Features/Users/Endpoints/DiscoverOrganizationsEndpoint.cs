using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Users.Services;
using Org.Shared.Common;
using Org.Shared.Features.Users;

namespace Org.Backend.Features.Users.Endpoints;

/// <summary>
/// Endpoint for discovering organizations
/// GET /api/users/me/discover/organizations
/// </summary>
public class DiscoverOrganizationsEndpoint : EndpointWithoutRequest<ApiResponse<List<DiscoverOrganizationDto>>>
{
    private readonly IUserService _userService;

    public DiscoverOrganizationsEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Get("/users/me/discover/organizations");
        Description(b => b
            .Produces<ApiResponse<List<DiscoverOrganizationDto>>>(200, "application/json")
            .Produces<ApiResponse<List<DiscoverOrganizationDto>>>(401, "application/json")
            .WithTags("Users"));
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
                Response = ApiResponse<List<DiscoverOrganizationDto>>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var result = await _userService.DiscoverOrganizationsAsync(userId, ct);
            Response = ApiResponse<List<DiscoverOrganizationDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<DiscoverOrganizationDto>>.ErrorResponse("Failed to discover organizations", new List<string> { ex.Message });
        }
    }
}
