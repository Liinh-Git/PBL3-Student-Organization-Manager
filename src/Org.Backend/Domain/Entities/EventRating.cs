// ---- Đánh giá sự kiện từ người tham dự ----
namespace Org.Backend.Domain.Entities;

/// <summary>
/// Đại diện cho đánh giá của user về một sự kiện.
/// - Rating: 1-5 stars
/// - Aspect: Overall/Organization/Content/Venue/Food
/// - Một user có thể đánh giá nhiều aspects của cùng một event
/// </summary>
public class EventRating : BaseEntity
{
    // FK → Event
    public Guid EventId { get; set; }
    // FK → User (người đánh giá)
    public Guid UserId { get; set; }
    // Điểm đánh giá (1-5)
    public int Rating { get; set; }
    // Khía cạnh đánh giá
    public Domain.Enums.RatingAspect Aspect { get; set; } = Domain.Enums.RatingAspect.Overall;
    // Nhận xét
    public string? Comment { get; set; }

    // Navigation
    public Event Event { get; set; } = null!;
    public User User { get; set; } = null!;
}
