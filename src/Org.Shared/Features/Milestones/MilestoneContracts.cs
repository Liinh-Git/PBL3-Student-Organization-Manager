// ---- DTO và request dùng chung giữa FE và BE cho module cột mốc sự kiện ----
namespace Org.Shared.Features.Milestones;

// ---- Thông tin đầy đủ một cột mốc (milestone) ----
// SortOrder: thứ tự hiển thị trong danh sách milestone của sự kiện (tăng dần)
// StartDate / EndDate: phải nằm trong khoảng StartDate–EndDate của Event cha
public sealed record MilestoneDto(
    Guid Id,
    Guid EventId,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate,
    int SortOrder,
    MilestoneStatus Status);

// ---- Yêu cầu tạo cột mốc mới cho một sự kiện ----
public sealed record CreateMilestoneRequest(
    Guid EventId,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate,
    int SortOrder);

// ---- Yêu cầu cập nhật thông tin và trạng thái cột mốc ----
public sealed record UpdateMilestoneRequest(
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate,
    int SortOrder,
    MilestoneStatus Status);

// ---- Phản hồi chi tiết một cột mốc ----
public sealed record GetMilestoneByIdResponse(MilestoneDto Data);

// ---- Phản hồi danh sách cột mốc (thường theo EventId) ----
public sealed record GetMilestonesResponse(IReadOnlyList<MilestoneDto> Items);
