// ---- Lớp nền cho mọi entity: khóa chính, audit timestamp, và soft-delete ----
namespace Org.Backend.Domain.Entities;

/// <summary>
/// Lớp trừu tượng được kế thừa bởi tất cả entity trong hệ thống.
/// - Id: Guid tự sinh, dùng làm primary key.
/// - CreatedAt / UpdatedAt: được AppDbContext.SaveChangesAsync() tự set.
/// - IsDeleted: soft-delete — EF Core áp dụng global query filter (!IsDeleted)
///   để dữ liệu đã xóa không xuất hiện trong truy vấn mặc định.
///   Dùng .IgnoreQueryFilters() khi cần truy cập dữ liệu đã xóa.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    // false = đang hoạt động, true = đã xóa mềm
    public bool IsDeleted { get; set; } = false;
}
