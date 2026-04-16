namespace Org.Shared.Contracts;

public sealed class MemberDto
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? RoleId { get; set; }
    public DateTime JoinDate { get; set; }
}

public sealed class AssignRoleRequest
{
    public Guid RoleId { get; set; }
}

public sealed class AssignDepartmentRequest
{
    public Guid DepartmentId { get; set; }
}

public sealed class CreateMemberRequest
{
    public Guid OrgId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
}
