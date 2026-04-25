// ---- Model nội bộ cho mock data store — ánh xạ 1:1 với các entity backend ----
// Các model này đơn giản hơn entity thật (không có audit fields) để dễ serialize JSON.
// MockDataSet chứa toàn bộ bảng mock, được load từ file .json khi khởi động.
namespace Org.Frontend.Services.Mocks.Models;

public sealed class MockDataSet
{
    public List<MockUser> Users { get; set; } = [];
    public List<MockOrganization> Organizations { get; set; } = [];
    public List<MockDepartment> Departments { get; set; } = [];
    public List<MockMember> Members { get; set; } = [];
    public List<MockEvent> Events { get; set; } = [];
    public List<MockEventMember> EventMembers { get; set; } = [];
    public List<MockAttendee> Attendees { get; set; } = [];
    public List<MockMilestone> Milestones { get; set; } = [];
    public List<MockEventCategory> EventCategories { get; set; } = [];
    public List<MockTask> Tasks { get; set; } = [];
    public List<MockRequest> Requests { get; set; } = [];
}

public sealed class MockRequest
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>JOIN hoặc OTHER</summary>
    public string RequestType { get; set; } = "JOIN";
    /// <summary>PENDING, APPROVED, REJECTED</summary>
    public string Status { get; set; } = "PENDING";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Title { get; set; }
    public string? Message { get; set; }
    // ---- Application detail (chỉ dùng cho RequestType = JOIN) ----
    public string? DesiredDepartment { get; set; }
    public string? DesiredPosition { get; set; }
    public string? Experience { get; set; }
    public string? Strengths { get; set; }
    public string? Reason { get; set; }
}

public sealed class MockUser
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? Bio { get; set; }
    public string? Status { get; set; }
    public bool? EmailNotificationsEnabled { get; set; }
    public bool? AppPushEnabled { get; set; }
    public bool? SmsAlertsEnabled { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class MockOrganization
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string OrgName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
    public string? Location { get; set; }
    public int TotalMembers { get; set; }
    public int Status { get; set; }
}

public sealed class MockDepartment
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string? Function { get; set; }
}

public sealed class MockMember
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? RoleId { get; set; }
    public DateTime JoinDate { get; set; }
}

public sealed class MockEvent
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string StatusLabel { get; set; } = "UPCOMING";
    public string? Location { get; set; }
    public int TotalSlots { get; set; }
    public string? ImageUrl { get; set; }
    public string? CompletionLabel { get; set; }
    public string? BudgetLabel { get; set; }
    public string? RiskLevel { get; set; }
    public int TotalFiles { get; set; }
    public decimal ActualSpending { get; set; }
}

public sealed class MockEventMember
{
    public Guid EventId { get; set; }
    public Guid MemberId { get; set; }
    public string? EventRole { get; set; }
}

public sealed class MockAttendee
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid? UserId { get; set; }
    public string? GuestName { get; set; }
    public string? Email { get; set; }
    public string? TicketType { get; set; }
    public DateTime? CheckInTime { get; set; }
    public string Status { get; set; } = "REGISTERED";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class MockMilestone
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int OrderIndex { get; set; }
}

public sealed class MockEventCategory
{
    public Guid Id { get; set; }
    public Guid MilestoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? LeadMemberId { get; set; }
    public int OrderIndex { get; set; }
    public string? Description { get; set; }
    public Guid? OwnerDepartmentId { get; set; }
}

public sealed class MockTask
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "TODO";
    public Guid? AssigneeMemberId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Note { get; set; }
}
