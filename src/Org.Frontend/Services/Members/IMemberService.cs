// ---- Interface service member — CRUD và phân công trong tổ chức ----
// Được implement bởi: MemberApiClient (thực) và MemberMockService (mock)
using Org.Shared.Contracts;
using FeatureCreateMemberRequest = Org.Shared.Features.Members.CreateMemberRequest;

namespace Org.Frontend.Services.Members;

public interface IMemberService
{
    // ---- Lấy danh sách thành viên trong tổ chức ----
    Task<List<MemberDto>> GetMembers(Guid orgId);
    // ---- Thêm thành viên mới (tự tạo account nếu chưa có) ----
    Task<MemberDto> CreateMember(Guid orgId, FeatureCreateMemberRequest req);
    // ---- Cập nhật vai trò thành viên ----
    Task AssignRole(Guid memberId, Guid roleId);
    // ---- Phân công phòng ban cho thành viên ----
    Task AssignDepartment(Guid memberId, Guid departmentId);
    // ---- Xóa mềm thành viên khỏi tổ chức ----
    Task DeleteMember(Guid memberId);
}
