using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.RolesPermissions.Services;
using Org.Backend.Features.RolesPermissions.Validators;
using Org.Shared.Common;
using Org.Shared.Features.RolesPermissions;

namespace Org.Backend.Features.RolesPermissions.Endpoints;

public class AssignRoleToMemberEndpoint : Endpoint<AssignRoleToMemberRequest, ApiResponse<bool>>
{
    private readonly IRoleService _roleService;

    public AssignRoleToMemberEndpoint(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public override void Configure()
    {
        Post("/organizations/{orgId}/members/{memberId}/role");
        Validator<AssignRoleToMemberRequestValidator>();
    }

    public override async Task HandleAsync(AssignRoleToMemberRequest req, CancellationToken ct)
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

            var orgId = Route<Guid>("orgId");
            var memberId = Route<Guid>("memberId");

            var result = await _roleService.AssignRoleToMemberAsync(orgId, memberId, userId, req, ct);

            Response = ApiResponse<bool>.SuccessResponse(result, "Role assigned to member successfully");
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
