using DriveApp.Enums;

namespace DriveApp.DTOs.Drivers;

public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? VehicleId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    public string? RejectionReason { get; set; }
}

public class UploadDocumentDto
{
    public DocumentType DocumentType { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? VehicleId { get; set; }
    // The actual file will be handled separately in the multipart form data
}

public class DocumentVerificationDto
{
    public bool IsVerified { get; set; }
    public string? RejectionReason { get; set; }
} 