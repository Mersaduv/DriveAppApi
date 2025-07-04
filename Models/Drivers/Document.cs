using DriveApp.Models.Core;
using DriveApp.Enums;

namespace DriveApp.Models.Drivers;

public class Document : BaseEntity
{
    public Guid? DriverId { get; set; }
    public Guid? VehicleId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    public string? RejectionReason { get; set; }
    
    // Navigation Properties
    public virtual Driver? Driver { get; set; }
    public virtual Vehicle? Vehicle { get; set; }
} 