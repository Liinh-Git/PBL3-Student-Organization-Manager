namespace Org.Backend.Domain.Enums;

/// <summary>
/// Loại activity feed log.
/// Storage: string.
/// </summary>
public enum ActivityType
{
    OrganizationCreated,
    MemberJoined,
    MemberLeft,
    EventCreated,
    EventUpdated,
    MilestoneCreated,
    CategoryCreated,
    TaskCreated,
    TaskUpdated,
    RequestSubmitted,
    RequestReviewed,
    NotificationSent,
    ResourceAdded,
    ReportGenerated,
    RoleChanged,
    DepartmentUpdated
}
