// ---- Thông báo gửi đến user ----
using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Đại diện cho một thông báo gửi đến user.
/// - ReceiverId: User nhận thông báo
/// - ActorId: User/System tạo thông báo (nullable - có thể là system notification)
/// - RelatedEntityId: ID của entity liên quan (FriendRequest, Member, Event, Task, etc.)
/// - RelatedEntityType: Loại entity để frontend biết navigate đến đâu
/// - ActionUrl: URL để frontend navigate khi click vào notification
/// - IsRead: Đã đọc chưa
/// - ReadAt: Thời điểm đọc
/// </summary>
public class Notification : BaseEntity
{
    // FK → User (người nhận)
    public Guid ReceiverId { get; set; }
    
    // Tiêu đề ngắn gọn
    public string Title { get; set; } = string.Empty;
    
    // Nội dung chi tiết
    public string Message { get; set; } = string.Empty;
    
    // Loại thông báo
    public NotificationType Type { get; set; }
    
    // Đã đọc chưa
    public bool IsRead { get; set; } = false;
    
    // FK → User (người/hệ thống tạo thông báo, nullable)
    public Guid? ActorId { get; set; }
    
    // ID của entity liên quan (FriendRequest, Member, Event, Task, etc.)
    public Guid? RelatedEntityId { get; set; }
    
    // Loại entity liên quan: "FriendRequest", "Member", "Event", "Task", "Post", etc.
    public string? RelatedEntityType { get; set; }
    
    // Action URL cho frontend navigate
    public string? ActionUrl { get; set; }
    
    // Icon/Image URL
    public string? IconUrl { get; set; }
    
    // Thời điểm đọc
    public DateTime? ReadAt { get; set; }
    
    // Navigation properties
    public User Receiver { get; set; } = null!;
    public User? Actor { get; set; }
}
