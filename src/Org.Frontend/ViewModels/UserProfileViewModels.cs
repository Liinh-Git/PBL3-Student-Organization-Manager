namespace Org.Frontend.ViewModels;

public sealed class UserProfileViewModel
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string ProfileVisibility { get; set; } = "Public";
    public bool IsOwnerView { get; set; }
    public bool CanViewFullProfile { get; set; }
    public bool IsProfileVisibleToViewer { get; set; }
    public string? HiddenReason { get; set; }
    public IReadOnlyList<UserProfileOrganizationSummaryViewModel> Organizations { get; set; } = [];
}

public sealed class UserProfileOrganizationSummaryViewModel
{
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string RoleName { get; set; } = "Member";
}
