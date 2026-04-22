// ---- DTO và request dùng chung giữa FE và BE cho module tổ chức ----
namespace Org.Shared.Features.Organizations;

// ---- Thông tin tóm tắt tổ chức (dùng cho danh sách hoặc dropdown) ----
public sealed record OrganizationSummaryDto(
    Guid Id,
    string Name,
    string? Description);

// ---- Thông tin đầy đủ một tổ chức ----
// AvatarUrl / CoverUrl: đường dẫn ảnh đại diện và ảnh bìa, có thể null
// FoundingDate: ngày thành lập (tùy chọn)
// TotalMembers: số thành viên đang active tại thời điểm query
// IsActive: false nếu tổ chức đã bị xóa mềm hoặc tạm ngưng
public sealed record OrganizationDto(
    Guid Id,
    string Name,
    string? Description,
    string? AvatarUrl,
    string? CoverUrl,
    DateOnly? FoundingDate,
    string? Location,
    int TotalMembers,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

// ---- Yêu cầu lấy danh sách tổ chức với tìm kiếm và phân trang ----
// IsActive: true = chỉ đang hoạt động, false = chỉ đã ngưng, null = tất cả
public sealed record GetOrganizationsRequest(
    string? Search   = null,
    bool? IsActive   = null,
    int Page         = 1,
    int PageSize     = 20);

// ---- Phản hồi danh sách tổ chức có kèm metadata phân trang ----
public sealed record GetOrganizationsResponse(
    IReadOnlyList<OrganizationSummaryDto> Items,
    int TotalCount   = 0,
    int Page         = 1,
    int PageSize     = 20,
    string? Search   = null,
    bool? IsActive   = null);

// ---- Phản hồi chi tiết một tổ chức ----
public sealed record GetOrganizationByIdResponse(OrganizationDto Data);

// ---- Yêu cầu tạo tổ chức mới ----
public sealed record CreateOrganizationRequest(
    string Name,
    string? Description,
    string? AvatarUrl,
    string? CoverUrl,
    DateOnly? FoundingDate,
    string? Location);

// ---- Yêu cầu cập nhật tổ chức (bao gồm kích hoạt / tạm ngưng) ----
public sealed record UpdateOrganizationRequest(
    string Name,
    string? Description,
    string? AvatarUrl,
    string? CoverUrl,
    DateOnly? FoundingDate,
    string? Location,
    bool IsActive);

// ---- Phản hồi lấy tổ chức mặc định của user hiện tại ----
// Dùng để FE xác định context tổ chức khi khởi động ứng dụng
public sealed record GetDefaultOrganizationResponse(OrganizationSummaryDto Data);
