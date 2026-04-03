// ---- DTO dùng chung giữa FE và BE cho module auth ----
namespace Org.Shared.Features.Auth;

public sealed class RegisterRequest
{
    // ---- Dữ liệu gửi lên để đăng ký ----
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class RegisterResponse
{
    // ---- Dữ liệu trả về sau khi đăng ký thành công ----
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    // ---- Dữ liệu gửi lên để đăng nhập ----
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    // ---- Dữ liệu trả về sau khi đăng nhập thành công ----
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class MeResponse
{
    // ---- Dữ liệu trả về khi gọi /api/auth/me ----
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
