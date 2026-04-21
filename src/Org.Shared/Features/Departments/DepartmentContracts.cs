// ---- DTO và request dùng chung giữa FE và BE cho module phòng ban ----
using Org.Shared.Features.Members;

namespace Org.Shared.Features.Departments;

// ---- Thông tin đầy đủ một phòng ban ----
// Code: mã viết tắt tự động tạo từ tên (vd: "KỸ THUẬT" → "KYTHUAT")
// ManagerMemberId: FK đến Member (không phải User) — người quản lý phải là thành viên của tổ chức
public sealed record DepartmentDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string? Description,
    Guid? ManagerMemberId,
    int MemberCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

// ---- Yêu cầu lấy danh sách phòng ban với tìm kiếm và phân trang ----
// IsActive: true = chỉ lấy đang hoạt động, false = chỉ lấy đã xóa, null = tất cả
public sealed record GetDepartmentsRequest(
    Guid OrganizationId,
    string? Search   = null,
    bool? IsActive   = null,
    int Page         = 1,
    int PageSize     = 20);

// ---- Phản hồi danh sách phòng ban có kèm metadata phân trang ----
public sealed record GetDepartmentsResponse(
    IReadOnlyList<DepartmentDto> Items,
    int TotalCount   = 0,
    int Page         = 1,
    int PageSize     = 20,
    string? Search   = null,
    bool? IsActive   = null);

// ---- Yêu cầu tạo phòng ban mới ----
// Code có thể để trống — hệ thống tự sinh từ Name nếu không cung cấp
public sealed record CreateDepartmentRequest(
    Guid OrganizationId,
    string Code,
    string Name,
    string? Description,
    Guid? ManagerMemberId);

// ---- Yêu cầu cập nhật thông tin phòng ban ----
// IsActive: false để xóa mềm, true để khôi phục
public sealed record UpdateDepartmentRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    Guid? ManagerMemberId);

// ---- Phản hồi chi tiết một phòng ban ----
public sealed record GetDepartmentByIdResponse(DepartmentDto Data);

// ---- Yêu cầu thay đổi người quản lý phòng ban (null = bỏ trống) ----
public sealed record UpdateDepartmentManagerRequest(Guid? ManagerMemberId);

// ---- Phản hồi danh sách thành viên trong phòng ban ----
public sealed record GetDepartmentMembersResponse(IReadOnlyList<MemberDto> Items);

// ---- Thông tin rút gọn một task để hiển thị trong overview phòng ban ----
public sealed record DepartmentTaskOverviewItemDto(
    Guid TaskId,
    string Title,
    TaskStatus Status,
    TaskPriority Priority,
    DateOnly? DueDate,
    Guid? AssigneeMemberId,
    string? AssigneeName);

// ---- Phản hồi tổng quan nhiệm vụ của phòng ban ----
// OpenTaskCount = TotalTasks - CompletedTaskCount
public sealed record GetDepartmentTasksOverviewResponse(
    Guid DepartmentId,
    int TotalTasks,
    int OpenTaskCount,
    int CompletedTaskCount,
    IReadOnlyList<DepartmentTaskOverviewItemDto> Items);
