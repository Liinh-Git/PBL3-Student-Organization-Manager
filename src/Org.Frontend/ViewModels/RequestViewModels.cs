// ---- ViewModels cho chức năng yêu cầu (Request) — dùng để truyền dữ liệu giữa service và UI ----
namespace Org.Frontend.ViewModels;

/// <summary>ViewModel hiển thị danh sách request (card view)</summary>
public sealed class RequestViewModel
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public List<string> Tags { get; set; } = [];
    public string RequestType { get; set; } = "JOIN";
    public string? Title { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
    // ---- Application detail (hiển thị trên card) ----
    public string? DesiredDepartment { get; set; }
    public string? DesiredPosition { get; set; }
    public string? Experience { get; set; }
}

/// <summary>ViewModel chi tiết đơn đăng ký (modal view cho chủ nhiệm)</summary>
public sealed class RequestDetailViewModel
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string RequestType { get; set; } = "JOIN";
    public string? Title { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
    // ---- Application detail fields ----
    public string? DesiredDepartment { get; set; }
    public string? DesiredPosition { get; set; }
    public string? Experience { get; set; }
    public string? Strengths { get; set; }
    public string? Reason { get; set; }
}

/// <summary>Form data sinh viên gửi khi đăng ký tham gia CLB</summary>
public sealed class JoinRequestFormViewModel
{
    public Guid OrgId { get; set; }
    public string? DesiredDepartmentName { get; set; }
    public string? DesiredPosition { get; set; }
    public string? Experience { get; set; }
    public string? Strengths { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class CreateOrganizationRequestViewModel
{
    public Guid OrgId { get; set; }
    public string RequestType { get; set; } = "GeneralOrgRequest";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
