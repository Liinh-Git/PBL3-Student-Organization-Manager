using System.Security.Claims;
using FastEndpoints;
using Org.Backend.Features.Departments.Services;
using Org.Backend.Features.Departments.Validators;
using Org.Shared.Common;
using Org.Shared.Features.Departments;

namespace Org.Backend.Features.Departments.Endpoints;

public class CreateDepartmentEndpoint : Endpoint<CreateDepartmentRequest, ApiResponse<DepartmentDto>>
{
    private readonly IDepartmentService _departmentService;

    public CreateDepartmentEndpoint(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    public override void Configure()
    {
        Post("/organizations/{orgId}/departments");
        Validator<CreateDepartmentRequestValidator>();
    }

    public override async Task HandleAsync(CreateDepartmentRequest req, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                HttpContext.Response.StatusCode = 401;
                Response = ApiResponse<DepartmentDto>.ErrorResponse("Invalid or missing user ID in token");
                return;
            }

            var orgId = Route<Guid>("orgId");

            var department = await _departmentService.CreateDepartmentAsync(orgId, userId, req, ct);

            Response = ApiResponse<DepartmentDto>.SuccessResponse(department, "Department created successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            HttpContext.Response.StatusCode = 403;
            Response = ApiResponse<DepartmentDto>.ErrorResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = 400;
            Response = ApiResponse<DepartmentDto>.ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            Response = ApiResponse<DepartmentDto>.ErrorResponse($"An error occurred: {ex.Message}");
        }
    }
}
