using DriveApp.Data;
using DriveApp.DTOs.Drivers;
using DriveApp.Enums;
using DriveApp.Models.Drivers;
using DriveApp.Services.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DriveApp.Services.Drivers;

public class DocumentService : BaseService, IDocumentService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _environment;
    
    public DocumentService(
        AppDbContext dbContext, 
        ILogger<DocumentService> logger,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment environment) : base(dbContext, logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _environment = environment;
    }
    
    public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync()
    {
        var documents = await _dbContext.Documents
            .Where(d => !d.IsDeleted)
            .ToListAsync();
            
        return documents.Select(d => d.ToDto());
    }
    
    public async Task<DocumentDto> GetDocumentByIdAsync(Guid id)
    {
        var document = await _dbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            
        if (document == null)
        {
            throw new KeyNotFoundException($"Document with ID {id} not found");
        }
        
        return document.ToDto();
    }
    
    public async Task<IEnumerable<DocumentDto>> GetDocumentsByDriverIdAsync(Guid driverId)
    {
        var documents = await _dbContext.Documents
            .Where(d => d.DriverId == driverId && !d.IsDeleted)
            .ToListAsync();
            
        return documents.Select(d => d.ToDto());
    }
    
    public async Task<IEnumerable<DocumentDto>> GetDocumentsByVehicleIdAsync(Guid vehicleId)
    {
        var documents = await _dbContext.Documents
            .Where(d => d.VehicleId == vehicleId && !d.IsDeleted)
            .ToListAsync();
            
        return documents.Select(d => d.ToDto());
    }
    
    public async Task<DocumentDto> UploadDocumentAsync(IFormFile file, UploadDocumentDto documentDto)
    {
        // Validate driver or vehicle ID is provided
        if (documentDto.DriverId == null && documentDto.VehicleId == null)
        {
            throw new ArgumentException("Either Driver ID or Vehicle ID must be provided");
        }
        
        // Validate driver exists if driver ID is provided
        if (documentDto.DriverId.HasValue)
        {
            var driverExists = await _dbContext.Drivers
                .AnyAsync(d => d.Id == documentDto.DriverId.Value && !d.IsDeleted);
                
            if (!driverExists)
            {
                throw new KeyNotFoundException($"Driver with ID {documentDto.DriverId} not found");
            }
        }
        
        // Validate vehicle exists if vehicle ID is provided
        if (documentDto.VehicleId.HasValue)
        {
            var vehicleExists = await _dbContext.Vehicles
                .AnyAsync(v => v.Id == documentDto.VehicleId.Value && !v.IsDeleted);
                
            if (!vehicleExists)
            {
                throw new KeyNotFoundException($"Vehicle with ID {documentDto.VehicleId} not found");
            }
        }
        
        // Check file size (limit to 10MB)
        const long maxFileSize = 10 * 1024 * 1024; // 10MB
        if (file.Length > maxFileSize)
        {
            throw new ArgumentException("File size exceeds the maximum allowed size of 10MB");
        }
        
        // Check file type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg", "application/pdf" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            throw new ArgumentException("File type not allowed. Allowed types: JPEG, PNG, JPG, PDF");
        }
        
        // Create directory for uploads if it doesn't exist
        var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads", "Documents");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }
        
        // Generate unique file name
        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine("Uploads", "Documents", fileName);
        var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
        
        // Save file to disk
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        // Create document record
        var document = new Document
        {
            DriverId = documentDto.DriverId,
            VehicleId = documentDto.VehicleId,
            DocumentType = documentDto.DocumentType,
            FilePath = filePath,
            OriginalFileName = file.FileName,
            FileSize = file.Length,
            MimeType = file.ContentType,
            IsVerified = false,
            CreatedBy = GetCurrentUserName()
        };
        
        await _dbContext.Documents.AddAsync(document);
        await _dbContext.SaveChangesAsync();
        
        return document.ToDto();
    }
    
    public async Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDocumentDto)
    {
        if (createDocumentDto.File == null)
        {
            throw new ArgumentException("File is required");
        }
        
        // Convert from CreateDocumentDto to UploadDocumentDto
        var uploadDto = new UploadDocumentDto
        {
            DriverId = createDocumentDto.DriverId,
            VehicleId = createDocumentDto.VehicleId,
            DocumentType = createDocumentDto.DocumentType
        };
        
        // Use the existing upload method
        return await UploadDocumentAsync(createDocumentDto.File, uploadDto);
    }
    
    public async Task<DocumentDto> UpdateDocumentAsync(Guid id, UpdateDocumentDto documentDto)
    {
        var document = await _dbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            
        if (document == null)
        {
            throw new KeyNotFoundException($"Document with ID {id} not found");
        }
        
        // Update the document properties
        document.DocumentType = documentDto.DocumentType;
        if (!string.IsNullOrWhiteSpace(documentDto.RejectionReason))
        {
            document.RejectionReason = documentDto.RejectionReason;
        }
        
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        return document.ToDto();
    }
    
    public async Task<bool> DeleteDocumentAsync(Guid id)
    {
        var document = await _dbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            
        if (document == null)
        {
            return false;
        }
        
        // Set as deleted in database
        document.IsDeleted = true;
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<DocumentDto> VerifyDocumentAsync(Guid id, DocumentVerificationDto verificationDto)
    {
        var document = await _dbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            
        if (document == null)
        {
            throw new KeyNotFoundException($"Document with ID {id} not found");
        }
        
        // Update verification status
        document.IsVerified = verificationDto.IsVerified;
        document.VerifiedAt = verificationDto.IsVerified ? DateTime.UtcNow : null;
        document.VerifiedBy = verificationDto.IsVerified ? GetCurrentUserName() : null;
        document.RejectionReason = !verificationDto.IsVerified ? verificationDto.RejectionReason : null;
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        return document.ToDto();
    }
    
    public async Task<bool> VerifyDocumentAsync(Guid id)
    {
        var document = await _dbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            
        if (document == null)
        {
            return false;
        }
        
        // Update verification status
        document.IsVerified = true;
        document.VerifiedAt = DateTime.UtcNow;
        document.VerifiedBy = GetCurrentUserName();
        document.RejectionReason = null;
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> RejectDocumentAsync(Guid id, string reason)
    {
        var document = await _dbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            
        if (document == null)
        {
            return false;
        }
        
        // Update rejection status
        document.IsVerified = false;
        document.VerifiedAt = null;
        document.VerifiedBy = null;
        document.RejectionReason = reason;
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    private string GetCurrentUserName()
    {
        if (_httpContextAccessor.HttpContext?.User?.Identity?.Name != null)
        {
            return _httpContextAccessor.HttpContext.User.Identity.Name;
        }
        
        return "System";
    }
} 