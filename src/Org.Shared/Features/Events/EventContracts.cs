// ---- DTO và request dùng chung giữa FE và BE cho module sự kiện ----
namespace Org.Shared.Features.Events;

// ---- Thông tin đầy đủ một sự kiện ----
public sealed record EventDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate,
    EventStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

// ---- Thông tin rút gọn của sự kiện kế hợp số liệu tổng hợp (dùng cho danh sách) ----
// MilestoneCount / CategoryCount / TaskCount: số lượng các hạng mục con
// CompletedTaskCount: số task đã hoàn thành, dùng để tính tiến độ
public sealed record EventTreeNodeDto(
    Guid Id,
    string Name,
    EventStatus Status,
    DateOnly StartDate,
    DateOnly EndDate,
    int MilestoneCount,
    int CategoryCount,
    int TaskCount,
    int CompletedTaskCount);

// ---- Yêu cầu lấy danh sách sự kiện của một tổ chức ----
public sealed record GetOrganizationEventsRequest(Guid OrganizationId);

// ---- Phản hồi danh sách sự kiện (dạng cây) ----
public sealed record GetOrganizationEventsResponse(IReadOnlyList<EventTreeNodeDto> Items);

// ---- Phản hồi chi tiết một sự kiện ----
public sealed record GetEventByIdResponse(EventDto Data);

// ---- Yêu cầu tạo sự kiện mới ----
public sealed record CreateEventRequest(
    Guid OrganizationId,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate);

// ---- Yêu cầu cập nhật sự kiện (bao gồm thay đổi trạng thái) ----
public sealed record UpdateEventRequest(
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly EndDate,
    EventStatus Status);
