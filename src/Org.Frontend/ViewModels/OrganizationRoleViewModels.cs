namespace Org.Frontend.ViewModels;

public sealed class OrganizationRoleViewModel
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyList<string> PermissionCodes { get; set; } = [];
    public bool IsProtected { get; set; }
    public int AssignedMemberCount { get; set; }
}

public sealed class PermissionOptionViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public sealed class UpsertOrganizationRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyList<string> PermissionCodes { get; set; } = [];
}
