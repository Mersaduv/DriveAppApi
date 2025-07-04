using DriveApp.DTOs.Drivers;
using Microsoft.AspNetCore.Http;

namespace DriveApp.Services.Drivers;

public interface IDocumentService
{
    Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync();
    Task<DocumentDto> GetDocumentByIdAsync(Guid id);
    Task<IEnumerable<DocumentDto>> GetDocumentsByDriverIdAsync(Guid driverId);
    Task<IEnumerable<DocumentDto>> GetDocumentsByVehicleIdAsync(Guid vehicleId);
    Task<DocumentDto> UploadDocumentAsync(IFormFile file, UploadDocumentDto documentDto);
    Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDocumentDto);
    Task<DocumentDto> UpdateDocumentAsync(Guid id, UpdateDocumentDto documentDto);
    Task<bool> DeleteDocumentAsync(Guid id);
    Task<DocumentDto> VerifyDocumentAsync(Guid id, DocumentVerificationDto verificationDto);
    Task<bool> VerifyDocumentAsync(Guid id);
    Task<bool> RejectDocumentAsync(Guid id, string reason);
} 