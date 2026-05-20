using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.RolesPermissions.Services;
using Org.Backend.Features.RolesPermissions.Validators;
using Org.Shared.Common;
using Org.Shared.Features.RolesPermissions;

namespace Org.Backend.Features.RolesPermissions.Endpoints;

public class UpdateRoleEndpoint : Endpoint<UpdateRoleRequest, ApiResponse<RoleDto>>
{
    private readonly IRoleService _roleService;

    public UpdateRoleEndpoint(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public override void Configure()
    {
        Put("/organizations/roles/{roleId}");
        Validator<UpdateRoleRequestValidator>();
    }

    public override async Task HandleAsync(UpdateRoleRequest req, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<RoleDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var roleId = Route<Guid>("roleId");

            var role = await _roleService.UpdateRoleAsync(roleId, userId, req, ct);

            Response = ApiResponse<RoleDto>.SuccessResponse(role, "Role updated successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<RoleDto>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<RoleDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<RoleDto>.ErrorResponse($"An error occurred: {ex.Message}");
        }
    }
}
