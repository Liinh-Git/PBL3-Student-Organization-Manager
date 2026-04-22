// ---- Enum dùng chung giữa BE và FE, liên quan đến thành viên, sự kiện, task, và tổ chức ----
namespace Org.Shared;

// ── Thành viên (Member) ────────────────────────────────────
// Thứ bậc vai trò: Member < Manager < VicePresident < President
public enum MemberRole
{
    Member        = 0,  // thành viên thường
    Manager       = 1,  // trưởng nhóm / quản lý
    VicePresident = 2,  // phó chủ tịch
    President     = 3   // chủ tịch
}

// ── Cột mốc sự kiện (Milestone) ───────────────────────────
public enum MilestoneStatus
{
    NotStarted = 0,  // chưa bắt đầu
    InProgress = 1,  // đang thực hiện
    Completed  = 2   // đã hoàn thành
}

// ── Nhiệm vụ (Task) ───────────────────────────────────────
public enum TaskStatus
{
    Todo       = 0,  // chưa làm
    InProgress = 1,  // đang làm
    Done       = 2   // đã xong
}

public enum TaskPriority
{
    Low    = 0,  // thấp
    Medium = 1,  // trung bình
    High   = 2   // cao
}

// ── Sự kiện (Event) ───────────────────────────────────────
public enum EventStatus
{
    Draft     = 0,  // bản nháp
    Planning  = 1,  // đang lên kế hoạch
    Ongoing   = 2,  // đang diễn ra
    Completed = 3   // đã kết thúc
}
