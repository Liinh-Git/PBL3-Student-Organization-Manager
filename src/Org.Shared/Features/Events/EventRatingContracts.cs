// ---- DTO và request dùng chung giữa FE và BE cho module đánh giá sự kiện ----
namespace Org.Shared.Features.Events;

// ---- Thông tin đánh giá sự kiện ----
public sealed record EventRatingDto(
    Guid Id,
    Guid EventId,
    Guid UserId,
    string UserName,
    string? UserAvatarUrl,
    int Rating,
    string Aspect,
    string? Comment,
    DateTimeOffset CreatedAtUtc);

// ---- Yêu cầu tạo đánh giá mới ----
public sealed record CreateEventRatingRequest(
    Guid EventId,
    int Rating,
    string Aspect,
    string? Comment);

// ---- Yêu cầu cập nhật đánh giá ----
public sealed record UpdateEventRatingRequest(
    int Rating,
    string? Comment);

// ---- Phản hồi danh sách đánh giá ----
public sealed record GetEventRatingsResponse(IReadOnlyList<EventRatingDto> Items);

// ---- Phản hồi chi tiết đánh giá ----
public sealed record GetEventRatingByIdResponse(EventRatingDto Data);

// ---- Thống kê đánh giá sự kiện ----
public sealed record EventRatingStatsDto(
    Guid EventId,
    float AverageOverall,
    float AverageOrganization,
    float AverageContent,
    float AverageVenue,
    float AverageFood,
    int TotalRatings,
    Dictionary<int, int> RatingDistribution);

// ---- Phản hồi thống kê đánh giá ----
public sealed record GetEventRatingStatsResponse(EventRatingStatsDto Data);
