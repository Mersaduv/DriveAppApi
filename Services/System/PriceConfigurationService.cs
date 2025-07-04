using Microsoft.EntityFrameworkCore;
using DriveApp.Data;
using DriveApp.DTOs.System;
using DriveApp.Models.System;
using DriveApp.Enums;
using DriveApp.Services.Helpers;

namespace DriveApp.Services.System;

public class PriceConfigurationService : BaseService, IPriceConfigurationService
{
    public PriceConfigurationService(AppDbContext dbContext, ILogger<PriceConfigurationService> logger)
        : base(dbContext, logger)
    {
    }
    
    public async Task<PriceConfigurationDto> GetPriceConfigurationByIdAsync(Guid id)
    {
        var config = await _dbContext.PriceConfigurations.FindAsync(id);
        if (config == null)
            return null;
            
        return config.ToDto();
    }
    
    public async Task<IEnumerable<PriceConfigurationDto>> GetAllPriceConfigurationsAsync()
    {
        var configs = await _dbContext.PriceConfigurations.ToListAsync();
        return configs.Select(c => c.ToDto());
    }
    
    public async Task<PriceConfigurationDto> GetPriceConfigurationByVehicleTypeAsync(VehicleType vehicleType)
    {
        var config = await _dbContext.PriceConfigurations
            .FirstOrDefaultAsync(c => c.VehicleType == vehicleType && c.IsActive);
            
        if (config == null)
            return null;
            
        return config.ToDto();
    }
    
    public async Task<PriceConfigurationDto> CreatePriceConfigurationAsync(CreatePriceConfigurationDto configDto)
    {
        // Check if configuration for vehicle type already exists
        var existingConfig = await _dbContext.PriceConfigurations
            .FirstOrDefaultAsync(c => c.VehicleType == configDto.VehicleType);
            
        if (existingConfig != null)
            throw new InvalidOperationException($"Price configuration for vehicle type {configDto.VehicleType} already exists");
            
        var config = configDto.ToEntity();
        
        await _dbContext.PriceConfigurations.AddAsync(config);
        await _dbContext.SaveChangesAsync();
        
        return config.ToDto();
    }
    
    public async Task<PriceConfigurationDto> UpdatePriceConfigurationAsync(Guid id, UpdatePriceConfigurationDto configDto)
    {
        var config = await _dbContext.PriceConfigurations.FindAsync(id);
        if (config == null)
            return null;
            
        config.UpdateFromDto(configDto);
        
        _dbContext.PriceConfigurations.Update(config);
        await _dbContext.SaveChangesAsync();
        
        return config.ToDto();
    }
    
    public async Task<bool> DeletePriceConfigurationAsync(Guid id)
    {
        var config = await _dbContext.PriceConfigurations.FindAsync(id);
        if (config == null)
            return false;
            
        // Soft delete
        config.IsDeleted = true;
        config.UpdatedAt = DateTime.UtcNow;
        
        _dbContext.PriceConfigurations.Update(config);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<decimal> CalculateTripPriceAsync(VehicleType vehicleType, decimal distance, int durationMinutes)
    {
        var config = await _dbContext.PriceConfigurations
            .FirstOrDefaultAsync(c => c.VehicleType == vehicleType && c.IsActive);
            
        if (config == null)
            throw new InvalidOperationException($"No active price configuration found for vehicle type {vehicleType}");
            
        // Calculate price based on distance and duration
        var basePrice = config.BasePrice;
        var distancePrice = config.PricePerKm * distance;
        var durationPrice = config.PricePerMinute * durationMinutes;
        
        var totalPrice = basePrice + distancePrice + durationPrice;
        
        // Apply minimum price if necessary
        if (totalPrice < config.MinimumPrice)
            totalPrice = config.MinimumPrice;
            
        return Math.Round(totalPrice, 2);
    }
} 