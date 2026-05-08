using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Departments.Services;
using Org.Shared.Common;
using Org.Shared.Features.Departments;

namespace Org.Backend.Features.Departments.Endpoints;

/// <summary>
/// Endpoint for getting organization departments
/// GET /api/organizations/{orgId}/departments
/// </summary>
public class GetOrganizationDepartmentsEndpoint : EndpointWithoutRequest<ApiResponse<List<DepartmentDto>>>
{
    private readonly IDepartmentService _departmentService;

    public GetOrganizationDepartmentsEndpoint(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    public override void Configure()
    {
        Get("/organizations/{orgId}/departments");
        Description(b => b
            .Produces<ApiResponse<List<DepartmentDto>>>(200, "application/json")
            .Produces<ApiResponse<List<DepartmentDto>>>(401, "application/json")
            .Produces<ApiResponse<List<DepartmentDto>>>(403, "application/json")
            .WithTags("Departments"));
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
                Response = ApiResponse<List<DepartmentDto>>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            // Get orgId from route
            var orgIdStr = Route<string>("orgId");
            if (string.IsNullOrEmpty(orgIdStr) || !Guid.TryParse(orgIdStr, out var orgId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<List<DepartmentDto>>.ErrorResponse("Invalid organization ID");
                return;
            }

            var result = await _departmentService.GetOrganizationDepartmentsAsync(orgId, userId, ct);
            Response = ApiResponse<List<DepartmentDto>>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<List<DepartmentDto>>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<List<DepartmentDto>>.ErrorResponse("Failed to get departments", new List<string> { ex.Message });
        }
    }
}
