using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Departments.Services;
using Org.Shared.Common;
using Org.Shared.Features.Departments;

namespace Org.Backend.Features.Departments.Endpoints;

/// <summary>
/// Endpoint for getting department by ID
/// GET /api/departments/{id}
/// </summary>
public class GetDepartmentByIdEndpoint : EndpointWithoutRequest<ApiResponse<DepartmentDto>>
{
    private readonly IDepartmentService _departmentService;

    public GetDepartmentByIdEndpoint(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    public override void Configure()
    {
        Get("/departments/{id}");
        Description(b => b
            .Produces<ApiResponse<DepartmentDto>>(200, "application/json")
            .Produces<ApiResponse<DepartmentDto>>(401, "application/json")
            .Produces<ApiResponse<DepartmentDto>>(403, "application/json")
            .Produces<ApiResponse<DepartmentDto>>(404, "application/json")
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
                Response = ApiResponse<DepartmentDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            // Get department ID from route
            var idStr = Route<string>("id");
            if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var departmentId))
            {
                HttpContext.Response.StatusCode = 400;
                Response = ApiResponse<DepartmentDto>.ErrorResponse("Invalid department ID");
                return;
            }

            var result = await _departmentService.GetDepartmentByIdAsync(departmentId, userId, ct);
            Response = ApiResponse<DepartmentDto>.SuccessResponse(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<DepartmentDto>.ErrorResponse(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<DepartmentDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<DepartmentDto>.ErrorResponse("Failed to get department", new List<string> { ex.Message });
        }
    }
}
