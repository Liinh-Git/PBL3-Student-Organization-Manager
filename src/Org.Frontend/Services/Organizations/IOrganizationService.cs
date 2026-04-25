using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Organizations;

public interface IOrganizationService
{
    Task<OrganizationDetailViewModel> GetOrganizationDetailAsync(Guid id, CancellationToken ct = default);
    Task<OrganizationDetailViewModel> CreateOrganizationAsync(CreateOrganizationViewModel model, CancellationToken ct = default);
}

public sealed class CreateOrganizationViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
}

public sealed class OrganizationDetailViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
    public string? Location { get; set; }
    public int TotalMembers { get; set; }
    public bool IsActive { get; set; }
    public DateTime FoundedDate { get; set; }
    
    // Additional data for Overview
    public List<OrganizationAdminViewModel> Admins { get; set; } = [];
    public List<OrganizationTimelineViewModel> Timeline { get; set; } = [];
}

public sealed class OrganizationAdminViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}

public sealed class OrganizationTimelineViewModel
{
    public string Month { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
