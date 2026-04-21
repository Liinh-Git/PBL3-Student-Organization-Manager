// ---- Tài khoản người dùng: xác thực + hồ sơ cá nhân ----
using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Đại diện cho tài khoản người dùng trong hệ thống.
/// - Kết hợp dữ liệu xác thực (PasswordHash, LastLogin) với hồ sơ cá nhân.
/// - Dob: ngày sinh (Date of Birth), lưu dưới dạng DateTime, tên cột ngắn.
/// - SocialLinks: chuỗi JSON dạng { "facebook": "url", "github": "url", ... }
/// - Một user có thể là thành viên của nhiều tổ chức khác nhau qua bảng Member.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    // Email là duy nhất trên toàn hệ thống (unique index)
    public string Email { get; set; } = string.Empty;
    // Mật khẩu đã băm bằng BCrypt, không lưu plain text
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    // Ngày sinh — Dob viết tắt của Date of Birth
    public DateTime? Dob { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    // Liên kết mạng xã hội lưu dạng JSON: { "platform": "url" }
    public string? SocialLinks { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    // Thời điểm đăng nhập lần cuối, null nếu chưa từng đăng nhập
    public DateTime? LastLogin { get; set; }

    // Navigation
    public ICollection<Member> Members { get; set; } = [];
    public ICollection<Attendee> Attendees { get; set; } = [];
    public ICollection<Request> Requests { get; set; } = [];
}
