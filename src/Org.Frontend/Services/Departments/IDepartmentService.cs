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
    // ---- Lấy tổng quan nhiệm vụ theo phòng ban ----
    Task<DepartmentTasksOverviewViewModel> GetTasksOverviewAsync(Guid departmentId);
}
