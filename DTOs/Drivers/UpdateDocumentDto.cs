using DriveApp.Enums;

namespace DriveApp.DTOs.Drivers;

public class UpdateDocumentDto
{
    public DocumentType DocumentType { get; set; }
    public string? RejectionReason { get; set; }
} 