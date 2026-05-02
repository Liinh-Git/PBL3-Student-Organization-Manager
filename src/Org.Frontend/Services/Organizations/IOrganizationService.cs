namespace Org.Frontend.Services.Organizations;

public interface IOrganizationService
{
    Task<OrganizationOverviewViewModel> GetOrganizationOverviewAsync(Guid organizationId, CancellationToken ct = default);
    Task<OrganizationOverviewViewModel> UpdateOrganizationOverviewAsync(Guid organizationId, UpdateOrganizationOverviewRequest request, CancellationToken ct = default);
    Task<MyOrganizationsViewModel> GetMyOrganizationsAsync(CancellationToken ct = default);
    Task<OrganizationViewerPermissionViewModel> GetOrganizationViewerPermissionAsync(Guid organizationId, CancellationToken ct = default);
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
}

public sealed class OrganizationOverviewViewModel
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? Mission { get; set; }
    public string? Vision { get; set; }
    public IReadOnlyList<string> Tags { get; set; } = [];
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
    public string? Location { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public DateOnly? FoundedDate { get; set; }
    public int MemberCount { get; set; }
    public int EventCount { get; set; }
    public int UpcomingEventCount { get; set; }
    public int ActiveTaskCount { get; set; }
    public int MilestoneCount { get; set; }
    public DateTime? LastActivityAtUtc { get; set; }
    public OrganizationViewerPermissionViewModel ViewerPermission { get; set; } = new();
    public IReadOnlyList<OrganizationDepartmentSummaryViewModel> Departments { get; set; } = [];
    public IReadOnlyList<OrganizationTimelineItemViewModel> Timeline { get; set; } = [];
    public IReadOnlyList<OrganizationEventHighlightViewModel> HighlightEvents { get; set; } = [];
    public IReadOnlyList<OrganizationAdminViewModel> Leadership { get; set; } = [];
}

public sealed class OrganizationViewerPermissionViewModel
{
    public bool IsAuthenticated { get; set; }
    public bool IsMember { get; set; }
    public bool CanAccessWorkspace { get; set; }
    public bool CanEditOverview { get; set; }
    public string ViewerMode { get; set; } = "EXTERNAL";
    public string? MemberRole { get; set; }
}

public sealed class OrganizationDepartmentSummaryViewModel
{
    public Guid DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MemberCount { get; set; }
}

public sealed class OrganizationTimelineItemViewModel
{
    public string PeriodLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class OrganizationEventHighlightViewModel
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
}

public sealed class OrganizationAdminViewModel
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}

public sealed class UpdateOrganizationOverviewRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? Mission { get; set; }
    public string? Vision { get; set; }
    public IReadOnlyList<string> Tags { get; set; } = [];
    public string? Location { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
}

public sealed class MyOrganizationsViewModel
{
    public IReadOnlyList<MyOrganizationCardViewModel> LeadingOrganizations { get; set; } = [];
    public IReadOnlyList<MyOrganizationCardViewModel> ParticipatingOrganizations { get; set; } = [];
}

public sealed class MyOrganizationCardViewModel
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? AvatarUrl { get; set; }
    public string MembershipRole { get; set; } = "Member";
    public string MembershipStatus { get; set; } = "ACTIVE";
    public int MemberCount { get; set; }
    public int UpcomingEventCount { get; set; }
    public int ActiveTaskCount { get; set; }
    public DateTimeOffset JoinedAtUtc { get; set; }
    public DateTime? LastActivityAtUtc { get; set; }
    public IReadOnlyList<string> Tags { get; set; } = [];
    public bool CanAccessWorkspace { get; set; }
    public bool CanManage { get; set; }
}
