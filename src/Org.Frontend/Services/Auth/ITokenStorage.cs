// ---- Interface lưu trữ token (localStorage hoặc cách khác) ----
namespace Org.Frontend.Services.Auth;

public interface ITokenStorage
{
    // ---- Lấy access token hiện tại ----
    Task<string?> GetTokenAsync(CancellationToken ct = default);
    // ---- Lấy thời gian hết hạn token ----
    Task<DateTime?> GetTokenExpiryAsync(CancellationToken ct = default);
    // ---- Lưu token và thời gian hết hạn ----
    Task SaveTokenAsync(string token, DateTime expiresAtUtc, CancellationToken ct = default);
    // ---- Xóa token khi logout hoặc token hết hạn ----
    Task ClearAsync(CancellationToken ct = default);
}
