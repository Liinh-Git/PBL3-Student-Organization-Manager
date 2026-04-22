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
