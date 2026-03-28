using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Represents a user account in the system.
/// Combines authentication data (passwordHash, lastLogin) with profile data.
/// socialLinks stored as JSON (key = platform, value = URL).
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? Dob { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? SocialLinks { get; set; }  // stored as JSON: { "facebook": "url", ... }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime? LastLogin { get; set; }

    // Navigation
    public ICollection<Member> Members { get; set; } = [];
    public ICollection<Attendee> Attendees { get; set; } = [];
    public ICollection<Request> Requests { get; set; } = [];
}
