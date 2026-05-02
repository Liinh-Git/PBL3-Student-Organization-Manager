// ---- DTO và request dùng chung giữa FE và BE cho module hồ sơ người dùng ----
using Org.Shared;

namespace Org.Shared.Features.Users;

// ---- Thông tin đầy đủ hồ sơ người dùng ----
// SocialLinksJson: chuỗi JSON chứa các liên kết mạng xã hội (vd: {"github":"...","linkedin":"..."})
// Status: trạng thái tài khoản theo UserStatus enum bên BE (Active / Inactive / Banned)
// LastLoginAtUtc: null nếu chưa từng đăng nhập sau khi được tạo
// ProfileVisibility: Public / OrganizationOnly / Private
public sealed record UserProfileDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Address,
    string? AvatarUrl,
    string? Bio,
    string? SocialLinksJson,
    string Status,
    string ProfileVisibility,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    DateTimeOffset? LastLoginAtUtc);

// ---- Phản hồi hồ sơ của user đang đăng nhập ----
public sealed record GetCurrentUserProfileResponse(UserProfileDto Data);

// ---- Phản hồi hồ sơ user theo ID (dùng cho admin hoặc xem profile người khác) ----
public sealed record GetUserByIdResponse(UserProfileDto Data);

// ---- Yêu cầu cập nhật hồ sơ cá nhân (không thể đổi email qua endpoint này) ----
// SocialLinksJson: ghi đè toàn bộ JSON, null = xóa toàn bộ liên kết
// ProfileVisibility: Public / OrganizationOnly / Private
public sealed record UpdateCurrentUserProfileRequest(
    string FullName,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Address,
    string? AvatarUrl,
    string? Bio,
    string? SocialLinksJson,
    string? ProfileVisibility = null);

// ---- Yêu cầu đổi mật khẩu ----
public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

// ---- Yêu cầu gửi lời mời kết bạn ----
public sealed record SendFriendRequestRequest(Guid ReceiverId);

// ---- Thông tin lời mời kết bạn ----
public sealed record FriendRequestDto(
    Guid Id,
    Guid SenderId,
    string SenderName,
    string? SenderAvatarUrl,
    Guid ReceiverId,
    string ReceiverName,
    string? ReceiverAvatarUrl,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? RespondedAtUtc);

// ---- Phản hồi danh sách lời mời kết bạn ----
public sealed record GetFriendRequestsResponse(IReadOnlyList<FriendRequestDto> Items);

// ---- Thông tin bạn bè ----
public sealed record FriendDto(
    Guid UserId,
    string FullName,
    string? AvatarUrl,
    string? Bio,
    DateTimeOffset FriendsSinceUtc);

// ---- Phản hồi danh sách bạn bè ----
public sealed record GetFriendsResponse(IReadOnlyList<FriendDto> Items);

// ---- Phản hồi batch user profiles ----
public sealed record GetUserProfilesBatchResponse(IReadOnlyList<UserProfileDto> Items);

// ---- Dữ liệu tổ chức của user đang đăng nhập (dùng cho dashboard user) ----
public sealed record MyOrganizationDto(
    Guid OrganizationId,
    string OrganizationName,
    string? OrganizationDescription,
    string? OrganizationAvatarUrl,
    DateTimeOffset JoinedAtUtc,
    string MemberRole);

// ---- Phản hồi danh sách tổ chức mà user đang tham gia ----
public sealed record GetMyOrganizationsResponse(IReadOnlyList<MyOrganizationDto> Items);

// ---- Dữ liệu sự kiện user đã đăng ký theo ngữ nghĩa Attendee ----
public sealed record MyRegisteredEventDto(
    Guid EventId,
    Guid OrganizationId,
    string OrganizationName,
    string EventName,
    string? EventDescription,
    DateOnly StartDate,
    DateOnly EndDate,
    EventStatus EventStatus,
    string RegistrationStatus,
    DateTimeOffset RegisteredAtUtc,
    string? Location = null,
    string? EventImageUrl = null);

// ---- Phản hồi danh sách sự kiện user đã đăng ký ----
public sealed record GetMyRegisteredEventsResponse(IReadOnlyList<MyRegisteredEventDto> Items);

// ---- Dữ liệu tổ chức đề xuất cho user dashboard ----
public sealed record SuggestedOrganizationDto(
    Guid OrganizationId,
    string OrganizationName,
    string? OrganizationDescription,
    string OrganizationImageUrl,
    int TotalMembers,
    string? Location,
    bool IsActive);

// ---- Phản hồi danh sách tổ chức đề xuất ----
public sealed record GetSuggestedOrganizationsResponse(IReadOnlyList<SuggestedOrganizationDto> Items);

// ---- Dữ liệu sự kiện đề xuất cho user dashboard ----
public sealed record SuggestedEventDto(
    Guid EventId,
    Guid OrganizationId,
    string OrganizationName,
    string EventName,
    string? EventDescription,
    DateOnly StartDate,
    DateOnly EndDate,
    EventStatus EventStatus,
    string? Location,
    string EventImageUrl,
    int RegisteredCount);

// ---- Phản hồi danh sách sự kiện đề xuất ----
public sealed record GetSuggestedEventsResponse(IReadOnlyList<SuggestedEventDto> Items);