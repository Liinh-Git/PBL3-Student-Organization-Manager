// ---- Interface service phòng ban — CRUD cho module Department ----
// Được implement bởi: DepartmentApiClient (thực) và DepartmentMockService (mock)
using Org.Shared.Contracts;
using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Departments;

public interface IDepartmentService
{
    // ---- Lấy danh sách phòng ban trong tổ chức ----
    Task<List<DepartmentDto>> GetDepartments(Guid orgId);
    // ---- Tạo phòng ban mới (Code tự sinh nếu để trống) ----
    Task<DepartmentDto> CreateDepartment(CreateDepartmentRequest req);
    // ---- Cập nhật thông tin phòng ban ----
    Task<DepartmentDto> UpdateDepartment(Guid id, UpdateDepartmentRequest req);
    // ---- Xóa mềm phòng ban ----
    Task DeleteDepartment(Guid id);
    // ---- Lấy thành viên đang thuộc phòng ban ----
    Task<List<MemberDto>> GetDepartmentMembersAsync(Guid departmentId);
    // ---- Gán/đổi quản lý phòng ban ----
    Task<DepartmentDto> AssignManagerAsync(Guid departmentId, Guid? managerMemberId);
    // ---- Thêm thành viên vào phòng ban ----
    Task AssignMemberAsync(Guid departmentId, Guid memberId);
    // ---- Gỡ thành viên khỏi phòng ban ----
    Task RemoveMemberAsync(Guid departmentId, Guid memberId);
    // ---- Lấy danh sách nhiệm vụ của phòng ban (department tasks) ----
    Task<List<DepartmentTaskDto>> GetDepartmentTasksAsync(Guid departmentId);
    // ---- Tạo nhiệm vụ mới cho phòng ban ----
    Task<DepartmentTaskDto> CreateDepartmentTaskAsync(Guid departmentId, CreateDepartmentTaskRequest request);
    // ---- Cập nhật nhiệm vụ phòng ban ----
    Task<DepartmentTaskDto> UpdateDepartmentTaskAsync(Guid taskId, UpdateDepartmentTaskRequest request);
    // ---- Xóa nhiệm vụ phòng ban ----
    Task DeleteDepartmentTaskAsync(Guid taskId);
    // ---- Đánh dấu hoàn thành nhiệm vụ phòng ban ----
    Task<DepartmentTaskDto> CompleteDepartmentTaskAsync(Guid taskId);
    // ---- Lấy tổng quan nhiệm vụ theo phòng ban ----
    Task<DepartmentTasksOverviewViewModel> GetTasksOverviewAsync(Guid departmentId);
}
