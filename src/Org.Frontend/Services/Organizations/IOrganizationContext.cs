// ---- Interface context tổ chức — cung cấp orgId hiện tại cho toàn bộ FE ----
// Được implement bởi: OrganizationApiClient (thực) và MockOrganizationContext (mock).
// UI inject interface này thay vì truy cập orgId trực tiếp để hỗ trợ multi-tenant.
namespace Org.Frontend.Services.Organizations;

public interface IOrganizationContext
{
    // ---- Lấy ID tổ chức đang được chọn (lazy resolve từ API nếu chưa có) ----
    Task<Guid> GetOrganizationIdAsync(CancellationToken ct = default);
    // ---- Xóa cache orgId khi đăng xuất hoặc chuyển tổ chức ----
    Task ResetAsync(CancellationToken ct = default);
}
