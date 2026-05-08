namespace Org.Shared.Features.Organizations;

// ============================================================
// REQUEST DTOs - Organization Write Operations
// ============================================================

/// <summary>
/// Request DTO for creating organization
/// </summary>
public record CreateOrganizationRequest
{
    /// <summary>
    /// Organization name (required, 2-200 characters)
    /// </summary>
    public required string OrgName { get; init; }

    /// <summary>
    /// Description (optional)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Avatar URL (optional)
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// Cover URL (optional)
    /// </summary>
    public string? CoverUrl { get; init; }

    /// <summary>
    /// Founding date (optional)
    /// </summary>
    public DateTime? FoundingDate { get; init; }

    /// <summary>
    /// Location (optional)
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Contact email (optional)
    /// </summary>
    public string? ContactEmail { get; init; }

    /// <summary>
    /// Contact phone (optional)
    /// </summary>
    public string? ContactPhone { get; init; }
}

/// <summary>
/// Request DTO for updating organization
/// </summary>
public record UpdateOrganizationRequest
{
    /// <summary>
    /// Organization name (required, 2-200 characters)
    /// </summary>
    public required string OrgName { get; init; }

    /// <summary>
    /// Description (optional)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Avatar URL (optional)
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// Cover URL (optional)
    /// </summary>
    public string? CoverUrl { get; init; }

    /// <summary>
    /// Founding date (optional)
    /// </summary>
    public DateTime? FoundingDate { get; init; }

    /// <summary>
    /// Location (optional)
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Contact email (optional)
    /// </summary>
    public string? ContactEmail { get; init; }

    /// <summary>
    /// Contact phone (optional)
    /// </summary>
    public string? ContactPhone { get; init; }
}
