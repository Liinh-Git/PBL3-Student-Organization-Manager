using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Users.Services;
using Org.Shared.Common;
using Org.Shared.Features.Users;

namespace Org.Backend.Features.Users.Endpoints;

/// <summary>
/// Endpoint for getting current user's events
/// GET /api/users/me/events
/// </summary>
public class GetMyEventsEndpoint : EndpointWithoutRequest<ApiResponse<List<MyEventDto>>>
{
    private readonly IUserService _userService;

    public GetMyEventsEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Get("/users/me/events");
        Description(b => b
            .Produces<ApiResponse<List<MyEventDto>>>(200, "application/json")
            .Produces<ApiResponse<List<MyEventDto>>>(401, "application/json")
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
                Response = ApiResponse<List<MyEventDto>>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var result = await _userService.GetMyEventsAsync(userId, ct);
            Response = ApiResponse<List<MyEventDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<MyEventDto>>.ErrorResponse("Failed to get events", new List<string> { ex.Message });
        }
    }
}
