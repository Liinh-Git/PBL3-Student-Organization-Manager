using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// File/asset được upload cho event.
/// Scope: SHOULD_HAVE_DB_V1_NO_WORKING_UI_YET.
/// UI/API working: no.
/// </summary>
public class DigitalAsset : BaseEntity
{
    public Guid EventId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public Guid? UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Event Event { get; set; } = null!;
    public virtual User? UploadedByUser { get; set; }
}
