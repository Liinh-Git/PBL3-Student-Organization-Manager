using Org.Backend.Domain.Enums;

namespace Org.Backend.Domain.Entities;

/// <summary>
/// A file (image, document, spreadsheet, video) uploaded and linked to an event.
/// uploadedBy references the member who uploaded the file.
/// </summary>
public class DigitalAsset : BaseEntity
{
    public Guid EventId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public Guid? UploadedBy { get; set; }  // FK → Member

    // Navigation
    public Event Event { get; set; } = null!;
    public Member? Uploader { get; set; }
}
