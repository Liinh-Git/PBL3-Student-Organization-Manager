// ---- ViewModel cho dashboard user (/home) ----
namespace Org.Frontend.ViewModels;

public sealed class UserDashboardViewModel
{
    public string DisplayName { get; set; } = "Nguoi dung";
    public string? Email { get; set; }
    public List<UserOrganizationViewModel> Organizations { get; set; } = [];
    public List<UserRegisteredEventViewModel> RegisteredEvents { get; set; } = [];
    public List<SuggestedOrganizationViewModel> SuggestedOrganizations { get; set; } = [];
    public List<SuggestedEventViewModel> SuggestedEvents { get; set; } = [];
}

public sealed class UserOrganizationViewModel
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTimeOffset JoinedAtUtc { get; set; }
    public string Role { get; set; } = "Member";
}

public sealed class UserRegisteredEventViewModel
{
    public Guid EventId { get; set; }
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string EventStatus { get; set; } = "DRAFT";
    public string RegistrationStatus { get; set; } = "REGISTERED";
    public DateTimeOffset RegisteredAtUtc { get; set; }
    public string? Location { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}

public sealed class SuggestedOrganizationViewModel
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
}

public sealed class SuggestedEventViewModel
{
    public Guid EventId { get; set; }
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string EventStatus { get; set; } = "DRAFT";
    public string? Location { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int RegisteredCount { get; set; }
}
