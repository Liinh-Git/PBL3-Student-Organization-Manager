namespace Org.Shared.Features.Departments;

// ============================================================
// REQUEST DTOs - Department Write Operations
// ============================================================

/// <summary>
/// Request DTO for creating department
/// </summary>
public record CreateDepartmentRequest
{
    /// <summary>
    /// Department name (required, 2-100 characters)
    /// </summary>
    public required string DepartmentName { get; init; }

    /// <summary>
    /// Description (optional)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Manager member ID (optional)
    /// </summary>
    public Guid? ManagerId { get; init; }
}

/// <summary>
/// Request DTO for updating department
/// </summary>
public record UpdateDepartmentRequest
{
    /// <summary>
    /// Department name (required, 2-100 characters)
    /// </summary>
    public required string DepartmentName { get; init; }

    /// <summary>
    /// Description (optional)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Manager member ID (optional)
    /// </summary>
    public Guid? ManagerId { get; init; }
}
