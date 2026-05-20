using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Users.Services;
using Org.Backend.Features.Users.Validators;
using Org.Shared.Common;
using Org.Shared.Features.Users;

namespace Org.Backend.Features.Users.Endpoints;

/// <summary>
/// Endpoint for updating user profile
/// PUT /api/users/me
/// </summary>
public class UpdateProfileEndpoint : Endpoint<UpdateUserProfileRequest, ApiResponse<UserProfileDto>>
{
    private readonly IUserService _userService;

    public UpdateProfileEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Put("/users/me");
        Validator<UpdateUserProfileRequestValidator>();
    }

    public override async Task HandleAsync(UpdateUserProfileRequest req, CancellationToken ct)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<UserProfileDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            // Update profile
            var userProfile = await _userService.UpdateProfileAsync(userId, req, ct);

            // Return success response
            Response = ApiResponse<UserProfileDto>.SuccessResponse(userProfile);
        }
        catch (KeyNotFoundException)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<UserProfileDto>.ErrorResponse("User not found");
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<UserProfileDto>.ErrorResponse(ex.Message);
        }
    }
}
