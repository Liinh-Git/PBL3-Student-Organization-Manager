namespace Org.Shared.Contracts;

public sealed class DepartmentDto
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string? Function { get; set; }
}

public sealed class CreateDepartmentRequest
{
    public Guid OrgId { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string? Function { get; set; }
}

public sealed class UpdateDepartmentRequest
{
    public string DeptName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string? Function { get; set; }
}
