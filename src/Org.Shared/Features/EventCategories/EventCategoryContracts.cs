// ---- DTO và request dùng chung giữa FE và BE cho module hạng mục sự kiện ----
namespace Org.Shared.Features.EventCategories;

// ---- Thông tin đầy đủ một hạng mục (category) trong cột mốc ----
// SortOrder: thứ tự hiển thị trong cột mốc cha (tăng dần)
// TaskCount / CompletedTaskCount: tổng hợp từ các task con, dùng để tính % hoàn thành
public sealed record EventCategoryDto(
    Guid Id,
    Guid MilestoneId,
    string Name,
    string? Description,
    int SortOrder,
    int TaskCount,
    int CompletedTaskCount,
    Guid? OwnerDepartmentId,
    Guid? LeadMemberId,
    string? LeadName);

// ---- Yêu cầu tạo hạng mục mới trong một cột mốc ----
public sealed record CreateEventCategoryRequest(
    Guid MilestoneId,
    string Name,
    string? Description,
    int SortOrder);

// ---- Yêu cầu cập nhật thông tin hạng mục (không thay đổi MilestoneId) ----
public sealed record UpdateEventCategoryRequest(
    string Name,
    string? Description,
    int SortOrder);

// ---- Phản hồi chi tiết một hạng mục ----
public sealed record GetEventCategoryByIdResponse(EventCategoryDto Data);

// ---- Phản hồi danh sách hạng mục (thường theo MilestoneId) ----
public sealed record GetEventCategoriesResponse(IReadOnlyList<EventCategoryDto> Items);
