using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.RolesPermissions.Services;
using Org.Shared.Common;

namespace Org.Backend.Features.RolesPermissions.Endpoints;

public class DeleteRoleEndpoint : EndpointWithoutRequest<ApiResponse<bool>>
{
    private readonly IRoleService _roleService;

    public DeleteRoleEndpoint(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public override void Configure()
    {
        Delete("/organizations/roles/{roleId}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<bool>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var roleId = Route<Guid>("roleId");

            var result = await _roleService.DeleteRoleAsync(roleId, userId, ct);

            Response = ApiResponse<bool>.SuccessResponse(result, "Role deleted successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<bool>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}");
        }
    }
}
