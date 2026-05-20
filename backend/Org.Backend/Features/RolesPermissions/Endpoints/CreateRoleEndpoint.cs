using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.RolesPermissions.Services;
using Org.Backend.Features.RolesPermissions.Validators;
using Org.Shared.Common;
using Org.Shared.Features.RolesPermissions;

namespace Org.Backend.Features.RolesPermissions.Endpoints;

public class CreateRoleEndpoint : Endpoint<CreateRoleRequest, ApiResponse<RoleDto>>
{
    private readonly IRoleService _roleService;

    public CreateRoleEndpoint(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public override void Configure()
    {
        Post("/organizations/{orgId}/roles");
        Validator<CreateRoleRequestValidator>();
    }

    public override async Task HandleAsync(CreateRoleRequest req, CancellationToken ct)
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

            var orgId = Route<Guid>("orgId");

            var role = await _roleService.CreateRoleAsync(orgId, userId, req, ct);

            Response = ApiResponse<RoleDto>.SuccessResponse(role, "Role created successfully");
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
