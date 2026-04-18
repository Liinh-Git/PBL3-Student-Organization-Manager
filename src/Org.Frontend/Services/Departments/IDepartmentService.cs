using Org.Shared.Contracts;

namespace Org.Frontend.Services.Departments;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetDepartments(Guid orgId);
    Task<DepartmentDto> CreateDepartment(CreateDepartmentRequest req);
    Task<DepartmentDto> UpdateDepartment(Guid id, UpdateDepartmentRequest req);
    Task DeleteDepartment(Guid id);
}
