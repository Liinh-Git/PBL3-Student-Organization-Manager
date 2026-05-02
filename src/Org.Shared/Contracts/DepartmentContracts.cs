// ---- DTO dùng chung giữa FE và FE (Frontend), không dùng trực tiếp với BE API ----
// Lưu ý: các DTO này được FE-layer sử dụng nội bộ qua mock service
// và có cấu trúc đơn giản hơn so với DTO trong Features/
namespace Org.Shared.Contracts;

// ---- Thông tin phòng ban (dùng cho FE mock và các component nội bộ) ----
public sealed class DepartmentDto
{
    public Guid Id { get; set; }
    public Guid OrgId { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string? Function { get; set; }
}

// ---- Yêu cầu tạo phòng ban mới (FE nội bộ) ----
public sealed class CreateDepartmentRequest
{
    public Guid OrgId { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string? Function { get; set; }
}

// ---- Yêu cầu cập nhật phòng ban (FE nội bộ) ----
public sealed class UpdateDepartmentRequest
{
    public string DeptName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string? Function { get; set; }
}

// ---- Task thuộc phòng ban (không phải event task) ----
public sealed class DepartmentTaskDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid DepartmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DeadlineAt { get; set; }
    // Chỉ dùng TODO | DONE cho department task
    public string Status { get; set; } = "TODO";
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Guid> AssigneeMemberIds { get; set; } = [];
}

public sealed class CreateDepartmentTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DeadlineAt { get; set; }
    public List<Guid> AssigneeMemberIds { get; set; } = [];
}

public sealed class UpdateDepartmentTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DeadlineAt { get; set; }
    public string Status { get; set; } = "TODO";
    public List<Guid> AssigneeMemberIds { get; set; } = [];
}
