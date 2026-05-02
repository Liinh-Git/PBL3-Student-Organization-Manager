// ---- Bài viết của tổ chức (cho trang khám phá) ----
using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// Đại diện cho bài viết do tổ chức đăng.
/// - PostType: General/Recruitment/Event/Announcement
/// - Visibility: Private/MembersOnly/Public
/// - CreatedBy: Member đã tạo bài viết
/// - TargetDepartmentId: null = tất cả phòng ban
/// </summary>
public class OrganizationPost : BaseEntity
{
    // FK → Organization
    public Guid OrgId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public PostType PostType { get; set; } = PostType.General;
    public PostVisibility Visibility { get; set; } = PostVisibility.MembersOnly;
    // FK → Department (null = tất cả phòng ban)
    public Guid? TargetDepartmentId { get; set; }
    // FK → Member (người tạo bài viết)
    public Guid CreatedBy { get; set; }
    // FK → Event (nếu là bài viết về sự kiện)
    public Guid? RelatedEventId { get; set; }
    // Số lượt xem
    public int ViewCount { get; set; } = 0;
    // Số lượt thích
    public int LikeCount { get; set; } = 0;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Department? TargetDepartment { get; set; }
    public Member Creator { get; set; } = null!;
    public Event? RelatedEvent { get; set; }
}
