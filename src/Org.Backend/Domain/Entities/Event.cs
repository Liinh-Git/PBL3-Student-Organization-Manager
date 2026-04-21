// ---- Sự kiện do tổ chức tổ chức ----
using Org.Backend.Domain.Enums;
using Org.Shared;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Đại diện cho một sự kiện do tổ chức tổ chức.
/// - Tags: chuỗi JSON array (vd: ["âm nhạc","ngoài trời"]) — lưu gọn, không chuẩn hóa.
/// - Budget: ngân sách kế hoạch, kiểu numeric(15,2) trong DB.
/// - AverageRating: điểm đánh giá bình quân tính từ EventReport (không tự cập nhật).
/// - TargetParticipants: số lượng người tham dự dự kiến.
/// - Cấu trúc phân cấp: Event → Milestone → EventCategory → OrgTask.
/// </summary>
public class Event : BaseEntity
{
    // FK → Organization
    public Guid OrgId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    // Ngân sách kế hoạch (numeric 15,2 trong DB)
    public decimal Budget { get; set; } = 0;
    public string? Location { get; set; }
    public int TargetParticipants { get; set; } = 0;
    // Tags lưu dạng JSON: ["music","outdoor",...]
    public string? Tags { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;
    // Điểm đánh giá trung bình từ EventReport (cập nhật thủ công)
    public float AverageRating { get; set; } = 0;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public ICollection<EventMember> EventMembers { get; set; } = [];
    public EventReport? EventReport { get; set; }
    public ICollection<Milestone> Milestones { get; set; } = [];
    public ICollection<Attendee> Attendees { get; set; } = [];
    public ICollection<DigitalAsset> DigitalAssets { get; set; } = [];
    public ICollection<Resource> Resources { get; set; } = [];
}
