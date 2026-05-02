// ---- Lời mời kết bạn giữa các user ----
using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Đại diện cho lời mời kết bạn giữa hai user.
/// - SenderId: người gửi lời mời
/// - ReceiverId: người nhận lời mời
/// - Status: trạng thái lời mời (Pending/Accepted/Rejected/Cancelled)
/// </summary>
public class FriendRequest : BaseEntity
{
    // FK → User (người gửi lời mời)
    public Guid SenderId { get; set; }
    // FK → User (người nhận lời mời)
    public Guid ReceiverId { get; set; }
    // Trạng thái lời mời
    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
    // Thời điểm phản hồi (accept/reject)
    public DateTime? RespondedAt { get; set; }

    // Navigation
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}
