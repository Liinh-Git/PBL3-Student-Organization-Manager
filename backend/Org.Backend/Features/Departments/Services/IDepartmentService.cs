using Org.Shared.Features.Departments;

namespace Org.Backend.Features.Departments.Services;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetOrganizationDepartmentsAsync(Guid orgId, Guid userId, CancellationToken ct = default);
    Task<DepartmentDto> GetDepartmentByIdAsync(Guid departmentId, Guid userId, CancellationToken ct = default);
    
    // Write operations
    Task<DepartmentDto> CreateDepartmentAsync(Guid orgId, Guid userId, CreateDepartmentRequest request, CancellationToken ct = default);
    Task<DepartmentDto> UpdateDepartmentAsync(Guid departmentId, Guid userId, UpdateDepartmentRequest request, CancellationToken ct = default);
    Task<bool> DeleteDepartmentAsync(Guid departmentId, Guid userId, CancellationToken ct = default);
}
