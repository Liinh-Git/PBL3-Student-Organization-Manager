using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Auth.Services;
using Org.Shared.Common;
using Org.Shared.Features.Auth;

namespace Org.Backend.Features.Auth.Endpoints;

/// <summary>
/// Endpoint for getting current authenticated user
/// GET /api/auth/me
/// </summary>
public class GetCurrentUserEndpoint : EndpointWithoutRequest<ApiResponse<CurrentUserResponse>>
{
    private readonly IAuthService _authService;

    public GetCurrentUserEndpoint(IAuthService authService)
    {
        _authService = authService;
    }

    public override void Configure()
    {
        Get("/auth/me");
        Description(b => b
            .Produces<ApiResponse<CurrentUserResponse>>(200, "application/json")
            .Produces<ApiResponse<CurrentUserResponse>>(401, "application/json")
            .Produces<ApiResponse<CurrentUserResponse>>(404, "application/json")
            .WithTags("Auth"));
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
                Response = ApiResponse<CurrentUserResponse>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var result = await _authService.GetCurrentUserAsync(userId, ct);
            Response = ApiResponse<CurrentUserResponse>.SuccessResponse(result);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<CurrentUserResponse>.ErrorResponse(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 401;
            Response = ApiResponse<CurrentUserResponse>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<CurrentUserResponse>.ErrorResponse("Failed to get current user", new List<string> { ex.Message });
        }
    }
}
