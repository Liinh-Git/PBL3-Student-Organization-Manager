using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Users.Services;
using Org.Shared.Common;
using Org.Shared.Features.Users;

namespace Org.Backend.Features.Users.Endpoints;

/// <summary>
/// Endpoint for getting current user profile
/// GET /api/users/me
/// </summary>
public class GetMeEndpoint : EndpointWithoutRequest<ApiResponse<UserProfileDto>>
{
    private readonly IUserService _userService;

    public GetMeEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Get("/users/me");
        Description(b => b
            .Produces<ApiResponse<UserProfileDto>>(200, "application/json")
            .Produces<ApiResponse<UserProfileDto>>(401, "application/json")
            .Produces<ApiResponse<UserProfileDto>>(404, "application/json")
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
                Response = ApiResponse<UserProfileDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var result = await _userService.GetMeAsync(userId, ct);
            Response = ApiResponse<UserProfileDto>.SuccessResponse(result);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<UserProfileDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<UserProfileDto>.ErrorResponse("Failed to get user profile", new List<string> { ex.Message });
        }
    }
}
