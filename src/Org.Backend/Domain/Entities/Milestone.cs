// ---- Cột mốc (phase/giai đoạn) trong một sự kiện ----
using Org.Backend.Domain.Enums;
using Org.Shared;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Đại diện cho một cột mốc (milestone/giai đoạn) trong cấu trúc sự kiện.
/// - Nhóm các EventCategory (hạng mục công việc) theo thời kỳ.
/// - OrderIndex: thứ tự hiển thị trong sự kiện, tăng dần từ 0.
/// - StartDate/EndDate: nên nằm trong khoảng của Event cha (không enforce ở DB level).
/// - Cấu trúc: Event → Milestone → EventCategory → OrgTask.
/// </summary>
public class Milestone : BaseEntity
{
    // FK → Event
    public Guid EventId { get; set; }
    // Tên cột mốc
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    // Thứ tự hiển thị trong sự kiện (tăng dần)
    public int OrderIndex { get; set; } = 0;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.NotStarted;

    // Navigation
    public Event Event { get; set; } = null!;
    public ICollection<EventCategory> Categories { get; set; } = [];
}
