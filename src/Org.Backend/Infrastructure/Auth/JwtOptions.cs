// ---- Cấu hình JWT đọc từ appsettings (Issuer, Audience, thời gian, khóa ký) ----
namespace Org.Backend.Infrastructure.Auth;

public sealed class JwtOptions
{
    // Tên section trong appsettings
    public const string SectionName = "Jwt";

    // Định danh hệ thống cấp token
    public string Issuer { get; set; } = "Org.Backend";
    // Đối tượng nhận token
    public string Audience { get; set; } = "Org.Frontend";
    // Thời gian sống của access token (phút)
    public int AccessTokenMinutes { get; set; } = 60;
    // Khóa ký token (bắt buộc mạnh khi production)
    public string SigningKey { get; set; } = "CHANGE_ME_TO_A_STRONG_RANDOM_SECRET_AT_LEAST_32_CHARS";
}
