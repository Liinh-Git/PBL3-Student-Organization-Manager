namespace Org.Frontend.Services.Overview;

public interface IOverviewService
{
    Task<OverviewPageViewModel> GetOverviewAsync(CancellationToken ct = default);
}

public sealed record OverviewPageViewModel(
    string DisplayName,
    IReadOnlyList<OverviewOrganizationItem> JoinedOrganizations,
    IReadOnlyList<OverviewEventItem> OrganizingEvents,
    IReadOnlyList<OverviewEventItem> AttendingEvents,
    IReadOnlyList<OverviewTaskItem> AssignedTasks);

public sealed record OverviewOrganizationItem(
    Guid OrganizationId,
    string Name,
    string? Description,
    string? AvatarUrl,
    string? Role,
    int MemberCount,
    int EventCount,
    int TaskCount);

public sealed record OverviewEventItem(
    Guid EventId,
    Guid OrganizationId,
    string EventTitle,
    string OrganizationName,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Location,
    string? Status,
    string? UserRole,
    string? ParticipationStatus,
    string? ImageUrl);

public sealed record OverviewTaskItem(
    Guid TaskId,
    string Title,
    string Description,
    DateTime? DeadlineUtc,
    string Status,
    string? Priority,
    Guid OrganizationId,
    string OrganizationName,
    Guid EventId,
    string EventName,
    Guid MilestoneId,
    string MilestoneName,
    Guid CategoryId,
    string CategoryName,
    Guid? DepartmentId,
    string? DepartmentName,
    IReadOnlyList<OverviewTaskAssigneeItem> CoAssignees);

public sealed record OverviewTaskAssigneeItem(
    Guid MemberId,
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    string? Role);
