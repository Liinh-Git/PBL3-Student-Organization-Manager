// ---- DTO và request dùng chung giữa FE và BE cho module bài viết ----
namespace Org.Shared.Features.Posts;

// ---- Thông tin bài viết tổ chức ----
public sealed record OrganizationPostDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    string? OrganizationAvatarUrl,
    string Title,
    string Content,
    string? ImageUrl,
    string PostType,
    string Visibility,
    Guid? TargetDepartmentId,
    string? TargetDepartmentName,
    Guid CreatedBy,
    string CreatorName,
    string? CreatorAvatarUrl,
    Guid? RelatedEventId,
    string? RelatedEventName,
    int ViewCount,
    int LikeCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

// ---- Yêu cầu tạo bài viết mới ----
public sealed record CreatePostRequest(
    Guid OrganizationId,
    string Title,
    string Content,
    string? ImageUrl,
    string PostType,
    string Visibility,
    Guid? TargetDepartmentId,
    Guid? RelatedEventId);

// ---- Yêu cầu cập nhật bài viết ----
public sealed record UpdatePostRequest(
    string Title,
    string Content,
    string? ImageUrl,
    string PostType,
    string Visibility,
    Guid? TargetDepartmentId,
    Guid? RelatedEventId);

// ---- Phản hồi danh sách bài viết ----
public sealed record GetPostsResponse(
    IReadOnlyList<OrganizationPostDto> Items,
    int TotalCount = 0,
    int Page = 1,
    int PageSize = 20);

// ---- Phản hồi chi tiết bài viết ----
public sealed record GetPostByIdResponse(OrganizationPostDto Data);

// ---- Bài viết đề xuất cho trang khám phá ----
public sealed record DiscoverPostDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    string? OrganizationAvatarUrl,
    string Title,
    string Content,
    string? ImageUrl,
    string PostType,
    Guid CreatedBy,
    string CreatorName,
    string? CreatorAvatarUrl,
    Guid? RelatedEventId,
    string? RelatedEventName,
    int ViewCount,
    int LikeCount,
    DateTimeOffset CreatedAtUtc);

// ---- Phản hồi bài viết khám phá ----
public sealed record GetDiscoverPostsResponse(IReadOnlyList<DiscoverPostDto> Items);
