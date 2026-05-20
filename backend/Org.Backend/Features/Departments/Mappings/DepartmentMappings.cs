using Org.Backend.Domain.Entities;
using Org.Shared.Features.Departments;

namespace Org.Backend.Features.Departments.Mappings;

public static class DepartmentMappings
{
    public static DepartmentDto ToDepartmentDto(this Department department, int memberCount = 0)
    {
        return new DepartmentDto
        {
            Id = department.Id,
            OrganizationId = department.OrgId,
            DeptName = department.DeptName,
            Code = department.Code,
            Function = department.Function,
            ManagerId = department.ManagerId,
            ManagerName = department.Manager?.User?.FullName,
            MemberCount = memberCount,
            Status = department.Status.ToString(),
            CreatedAtUtc = department.CreatedAt,
            UpdatedAtUtc = department.UpdatedAt ?? department.CreatedAt
        };
    }
}
