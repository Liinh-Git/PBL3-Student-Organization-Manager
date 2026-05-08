using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Users.Services;
using Org.Shared.Common;
using Org.Shared.Features.Users;

namespace Org.Backend.Features.Users.Endpoints;

/// <summary>
/// Endpoint for getting current user's organizations
/// GET /api/users/me/organizations
/// </summary>
public class GetMyOrganizationsEndpoint : EndpointWithoutRequest<ApiResponse<List<MyOrganizationDto>>>
{
    private readonly IUserService _userService;

    public GetMyOrganizationsEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Get("/users/me/organizations");
        Description(b => b
            .Produces<ApiResponse<List<MyOrganizationDto>>>(200, "application/json")
            .Produces<ApiResponse<List<MyOrganizationDto>>>(401, "application/json")
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
                Response = ApiResponse<List<MyOrganizationDto>>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var result = await _userService.GetMyOrganizationsAsync(userId, ct);
            Response = ApiResponse<List<MyOrganizationDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<MyOrganizationDto>>.ErrorResponse("Failed to get organizations", new List<string> { ex.Message });
        }
    }
}
