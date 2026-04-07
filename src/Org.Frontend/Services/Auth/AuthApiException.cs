// ---- Exception cho các lỗi gọi API auth ----
namespace Org.Frontend.Services.Auth;

public sealed class AuthApiException : Exception
{
    // Lưu HTTP status code để UI map thông báo
    public int StatusCode { get; }

    // ---- Tạo exception với message và status code ----
    public AuthApiException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
