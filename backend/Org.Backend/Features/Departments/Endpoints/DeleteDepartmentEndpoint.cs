using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Departments.Services;
using Org.Shared.Common;

namespace Org.Backend.Features.Departments.Endpoints;

public class DeleteDepartmentEndpoint : EndpointWithoutRequest<ApiResponse<bool>>
{
    private readonly IDepartmentService _departmentService;

    public DeleteDepartmentEndpoint(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    public override void Configure()
    {
        Delete("/departments/{id}");
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

            var departmentId = Route<Guid>("id");

            var result = await _departmentService.DeleteDepartmentAsync(departmentId, userId, ct);

            Response = ApiResponse<bool>.SuccessResponse(result, "Department deleted successfully");
        }
        catch (KeyNotFoundException)
        {
            HttpContext.Response.StatusCode = 404;
            Response = ApiResponse<bool>.ErrorResponse("Department not found");
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
