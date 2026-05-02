namespace Org.Backend.Domain.Enums;

// ── User ──────────────────────────────────────────────────
public enum UserStatus        { Active, Inactive, Banned }
public enum ProfileVisibility { Public, OrganizationOnly, Private }
public enum FriendRequestStatus { Pending, Accepted, Rejected, Cancelled }

// ── Organisation ──────────────────────────────────────────
public enum OrgStatus         { Active, Inactive }

// ── ActivityHistory ───────────────────────────────────────
public enum ActivityType      { EventCreated, MemberJoined, RoleChanged, RequestApproved, TaskCompleted, Other }

// ── Request ───────────────────────────────────────────────
public enum RequestType       { JoinClub, ApproveEvent, ResourceBorrow }
public enum RequestStatus     { Pending, Approved, Rejected }

// ── Resource ──────────────────────────────────────────────
public enum ResourceStatus    { Available, InUse, Unavailable }

// ── Attendee ──────────────────────────────────────────────
public enum AttendeeStatus    { Registered, Attended, Cancelled }

// ── DigitalAsset ──────────────────────────────────────────
public enum FileType          { Image, Document, Spreadsheet, Video }

// ── OrganizationPost ──────────────────────────────────────
public enum PostType          { General, Recruitment, Event, Announcement }
public enum PostVisibility    { Private, MembersOnly, Public }

// ── Event ─────────────────────────────────────────────────
public enum EventVisibility   { Private, MembersOnly, Public }

// ── EventRating ───────────────────────────────────────────
public enum RatingAspect      { Overall, Organization, Content, Venue, Food }

// ── Notification ──────────────────────────────────────────
public enum NotificationType
{
    // === FRIEND SYSTEM ===
    FriendRequestReceived = 1,      // Nhận lời mời kết bạn
    FriendRequestAccepted = 2,      // Lời mời kết bạn được chấp nhận
    FriendRequestRejected = 3,      // Lời mời kết bạn bị từ chối
    
    // === ORGANIZATION MEMBERSHIP ===
    JoinRequestReceived = 10,       // Org nhận được yêu cầu tham gia
    JoinRequestApproved = 11,       // Yêu cầu tham gia được duyệt
    JoinRequestRejected = 12,       // Yêu cầu tham gia bị từ chối
    InvitationReceived = 13,        // Nhận lời mời tham gia org
    MemberRoleChanged = 14,         // Vai trò trong org thay đổi
    MemberRemoved = 15,             // Bị xóa khỏi org
    
    // === EVENT SYSTEM ===
    EventInvitation = 20,           // Được mời tham gia sự kiện
    EventRegistrationApproved = 21, // Đăng ký sự kiện được duyệt
    EventRegistrationRejected = 22, // Đăng ký sự kiện bị từ chối
    EventUpdated = 23,              // Sự kiện có thay đổi
    EventCancelled = 24,            // Sự kiện bị hủy
    EventReminder = 25,             // Nhắc nhở sự kiện sắp diễn ra
    
    // === TASK SYSTEM ===
    TaskAssigned = 30,              // Được giao task
    TaskDeadlineReminder = 31,      // Nhắc deadline task
    TaskStatusChanged = 32,         // Trạng thái task thay đổi
    TaskCommentAdded = 33,          // Có comment mới trên task
    
    // === POST & SOCIAL ===
    PostLiked = 40,                 // Bài viết được thích
    PostCommented = 41,             // Bài viết có comment mới
    PostMentioned = 42,             // Được mention trong bài viết
    
    // === SYSTEM ===
    SystemAnnouncement = 50,        // Thông báo hệ thống
    SystemMaintenance = 51,         // Bảo trì hệ thống
    
    // === OTHER ===
    General = 99                    // Thông báo chung
}
