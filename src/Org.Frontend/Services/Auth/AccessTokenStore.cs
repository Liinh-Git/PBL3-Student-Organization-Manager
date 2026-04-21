// ---- Lưu access token trong memory (scoped) để tránh gọi JS mỗi request ----
// Được AuthHeaderDelegatingHandler đọc thay vì gọi localStorage mỗi lần.
// FrontendAuthStateProvider ghi vào đây sau khi đăng nhập/khởi tạo auth.
namespace Org.Frontend.Services.Auth;

// ---- Hợp đồng interface lưu token trong bộ nhớ ----
public interface IAccessTokenStore
{
    // Token hiện tại (null nếu chưa đăng nhập hoặc đã đăng xuất)
    string? AccessToken { get; set; }
    // Thời điểm hết hạn theo UTC (null nếu chưa có token)
    DateTime? ExpiresAtUtc { get; set; }
}

// ---- Implementation mặc định: lưu đơn giản trong bộ nhớ Scoped ----
public sealed class AccessTokenStore : IAccessTokenStore
{
    public string? AccessToken { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
}
