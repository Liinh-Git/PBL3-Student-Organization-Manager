using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Users.Services;
using Org.Backend.Features.Users.Validators;
using Org.Shared.Common;
using Org.Shared.Features.Users;

namespace Org.Backend.Features.Users.Endpoints;

/// <summary>
/// Endpoint for changing user password
/// PUT /api/users/me/change-password
/// </summary>
public class ChangePasswordEndpoint : Endpoint<ChangePasswordRequest, ApiResponse<bool>>
{
    private readonly IUserService _userService;

    public ChangePasswordEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Put("/users/me/change-password");
        Validator<ChangePasswordRequestValidator>();
    }

    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<bool>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            // Change password
            await _userService.ChangePasswordAsync(userId, req, ct);

            // Return success response
            Response = ApiResponse<bool>.SuccessResponse(true, "Password changed successfully");
        }
        catch (KeyNotFoundException)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<bool>.ErrorResponse("User not found");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 401;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }
}
