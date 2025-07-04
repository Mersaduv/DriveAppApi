using DriveApp.Enums;
using Microsoft.AspNetCore.Http;

namespace DriveApp.DTOs.Drivers;

public class CreateDocumentDto
{
    public DocumentType DocumentType { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? VehicleId { get; set; }
    public IFormFile File { get; set; } = null!;
} 