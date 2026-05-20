# Departments Services

## IDepartmentService / DepartmentService
**Methods**:
- `Task<List<DepartmentDto>> ListDepartmentsAsync(Guid orgId, Guid userId)`
- `Task<DepartmentDto> CreateDepartmentAsync(Guid orgId, CreateDepartmentRequest request, Guid userId)`
- `Task<DepartmentDto> GetDepartmentAsync(Guid deptId, Guid userId)`
- `Task<DepartmentDto> UpdateDepartmentAsync(Guid deptId, UpdateDepartmentRequest request, Guid userId)`
- `Task DeleteDepartmentAsync(Guid deptId, Guid userId)`

## NOT Implemented in Phase 3C
- ❌ No real service implementations
