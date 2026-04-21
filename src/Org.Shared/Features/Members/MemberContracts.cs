// ---- DTO và request dùng chung giữa FE và BE cho module thành viên ----
namespace Org.Shared.Features.Members;

// ---- Thông tin đầy đủ một thành viên trong tổ chức ----
// StudentCode: mã sinh viên, định danh ngắn dạng "MEM-XXXXXXXX"
// Role: vai trò hiện tại trong tổ chức (Member / Manager / VicePresident / President)
// IsActive: false nếu thành viên đã bị xóa mềm
public sealed record MemberDto(
    Guid Id,
    Guid OrganizationId,
    Guid? DepartmentId,
    string StudentCode,
    string FullName,
    string Email,
    MemberRole Role,
    bool IsActive,
    DateTimeOffset JoinedAtUtc);

// ---- Yêu cầu lấy danh sách thành viên của một tổ chức ----
public sealed record GetMembersRequest(Guid OrganizationId);

// ---- Phản hồi danh sách thành viên ----
public sealed record GetMembersResponse(IReadOnlyList<MemberDto> Items);

// ---- Yêu cầu thêm thành viên mới vào tổ chức ----
// Nếu email đã tồn tại trong DB, hệ thống sẽ tái kích hoạt tài khoản cũ thay vì tạo mới
public sealed record CreateMemberRequest(
    string FullName,
    string Email,
    Guid? DepartmentId);

// ---- Yêu cầu cập nhật vai trò một thành viên ----
public sealed record UpdateMemberRoleRequest(MemberRole Role);

// ---- Yêu cầu chuyển thành viên sang phòng ban khác (null = bỏ khỏi phòng ban hiện tại) ----
public sealed record UpdateMemberDepartmentRequest(Guid? DepartmentId);
