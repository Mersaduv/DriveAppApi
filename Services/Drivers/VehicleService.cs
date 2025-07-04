using DriveApp.Data;
using DriveApp.DTOs.Drivers;
using DriveApp.Models.Drivers;
using DriveApp.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DriveApp.Services.Drivers;

public class VehicleService : BaseService, IVehicleService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public VehicleService(
        AppDbContext dbContext, 
        ILogger<VehicleService> logger,
        IHttpContextAccessor httpContextAccessor) : base(dbContext, logger)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<VehicleDto> GetVehicleByIdAsync(Guid id)
    {
        var vehicle = await _dbContext.Vehicles
            .Include(v => v.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
            
        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle with ID {id} not found");
        }
        
        return vehicle.ToDto();
    }
    
    public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
    {
        var vehicles = await _dbContext.Vehicles
            .Include(v => v.Documents.Where(d => !d.IsDeleted))
            .Where(v => !v.IsDeleted)
            .ToListAsync();
            
        return vehicles.Select(v => v.ToDto());
    }
    
    public async Task<IEnumerable<VehicleDto>> GetVehiclesByDriverIdAsync(Guid driverId)
    {
        // Check if driver exists
        var driverExists = await _dbContext.Drivers
            .AnyAsync(d => d.Id == driverId && !d.IsDeleted);
            
        if (!driverExists)
        {
            throw new KeyNotFoundException($"Driver with ID {driverId} not found");
        }
        
        var vehicles = await _dbContext.Vehicles
            .Include(v => v.Documents.Where(d => !d.IsDeleted))
            .Where(v => v.DriverId == driverId && !v.IsDeleted)
            .ToListAsync();
            
        return vehicles.Select(v => v.ToDto());
    }
    
    public async Task<VehicleDto> CreateVehicleAsync(Guid driverId, CreateVehicleDto vehicleDto)
    {
        // Check if driver exists
        var driverExists = await _dbContext.Drivers
            .AnyAsync(d => d.Id == driverId && !d.IsDeleted);
            
        if (!driverExists)
        {
            throw new KeyNotFoundException($"Driver with ID {driverId} not found");
        }
        
        // Create vehicle entity
        var vehicle = vehicleDto.ToEntity(driverId);
        
        vehicle.CreatedBy = GetCurrentUserName();
        
        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.SaveChangesAsync();
        
        return vehicle.ToDto();
    }
    
    public async Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleDto vehicleDto)
    {
        var vehicle = await _dbContext.Vehicles
            .Include(v => v.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
            
        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle with ID {id} not found");
        }
        
        // Update vehicle properties
        vehicle.UpdateFromDto(vehicleDto);
        vehicle.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        return vehicle.ToDto();
    }
    
    public async Task<bool> DeleteVehicleAsync(Guid id)
    {
        var vehicle = await _dbContext.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
            
        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle with ID {id} not found");
        }
        
        // Mark as deleted
        vehicle.IsDeleted = true;
        vehicle.UpdatedAt = DateTime.UtcNow;
        vehicle.UpdatedBy = GetCurrentUserName();
        
        // Also mark related documents as deleted
        var relatedDocuments = await _dbContext.Documents
            .Where(d => d.VehicleId == id && !d.IsDeleted)
            .ToListAsync();
            
        foreach (var document in relatedDocuments)
        {
            document.IsDeleted = true;
            document.UpdatedAt = DateTime.UtcNow;
            document.UpdatedBy = GetCurrentUserName();
        }
        
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> VerifyVehicleAsync(Guid id)
    {
        var vehicle = await _dbContext.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
            
        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle with ID {id} not found");
        }
        
        // Mark as verified
        vehicle.IsVerified = true;
        vehicle.UpdatedAt = DateTime.UtcNow;
        vehicle.UpdatedBy = GetCurrentUserName();
        
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