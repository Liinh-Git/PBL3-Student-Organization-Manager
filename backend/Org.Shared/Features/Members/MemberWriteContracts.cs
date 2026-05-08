namespace Org.Shared.Features.Members;

// ============================================================
// REQUEST DTOs - Member Write Operations
// ============================================================

/// <summary>
/// Request DTO for adding member to organization
/// </summary>
public record AddMemberRequest
{
    /// <summary>
    /// User ID to add as member (required)
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Role ID to assign (optional, defaults to Member role)
    /// </summary>
    public Guid? RoleId { get; init; }

    /// <summary>
    /// Department ID to assign (optional)
    /// </summary>
    public Guid? DepartmentId { get; init; }

    /// <summary>
    /// Student code (optional)
    /// </summary>
    public string? StudentCode { get; init; }
}

/// <summary>
/// Request DTO for updating member department
/// </summary>
public record UpdateMemberDepartmentRequest
{
    /// <summary>
    /// Department ID to assign (required, use null to remove department)
    /// </summary>
    public Guid? DepartmentId { get; init; }
}
