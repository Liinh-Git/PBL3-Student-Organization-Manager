using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Tài khoản người dùng, auth profile, nguồn của membership, attendee, notification và friend flow.
/// Scope: MUST_HAVE_DB_V1.
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
    public string? SocialLinks { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public ProfileVisibility? ProfileVisibility { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool EmailConfirmed { get; set; } = false;

    // Navigation properties
    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
    public virtual ICollection<FriendRequest> SentFriendRequests { get; set; } = new List<FriendRequest>();
    public virtual ICollection<FriendRequest> ReceivedFriendRequests { get; set; } = new List<FriendRequest>();
    public virtual ICollection<Notification> NotificationsReceived { get; set; } = new List<Notification>();
    public virtual ICollection<Notification> NotificationsActedAsActor { get; set; } = new List<Notification>();
    public virtual ICollection<Attendee> Attendees { get; set; } = new List<Attendee>();
    public virtual ICollection<EventRating> EventRatings { get; set; } = new List<EventRating>();
    public virtual ICollection<DigitalAsset> UploadedDigitalAssets { get; set; } = new List<DigitalAsset>();
}
