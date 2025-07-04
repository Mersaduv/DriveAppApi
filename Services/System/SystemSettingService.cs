using Microsoft.EntityFrameworkCore;
using DriveApp.Data;
using DriveApp.DTOs.System;
using DriveApp.Models.System;
using DriveApp.Services.Helpers;

namespace DriveApp.Services.System;

public class SystemSettingService : BaseService, ISystemSettingService
{
    public SystemSettingService(AppDbContext dbContext, ILogger<SystemSettingService> logger)
        : base(dbContext, logger)
    {
    }
    
    public async Task<SystemSettingDto> GetSettingByIdAsync(Guid id)
    {
        var setting = await _dbContext.SystemSettings.FindAsync(id);
        if (setting == null)
            return null;
            
        return setting.ToDto();
    }
    
    public async Task<SystemSettingDto?> GetSettingByKeyAsync(string key)
    {
        var setting = await _dbContext.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null)
            return null;
            
        return setting.ToDto();
    }
    
    public async Task<IEnumerable<SystemSettingDto>> GetAllSettingsAsync()
    {
        var settings = await _dbContext.SystemSettings.ToListAsync();
        return settings.Select(s => s.ToDto());
    }
    
    public async Task<IEnumerable<SystemSettingDto>> GetPublicSettingsAsync()
    {
        var settings = await _dbContext.SystemSettings
            .Where(s => s.IsPublic)
            .ToListAsync();
            
        return settings.Select(s => s.ToDto());
    }
    
    public async Task<IEnumerable<SystemSettingDto>> GetSettingsByCategoryAsync(string category)
    {
        var settings = await _dbContext.SystemSettings
            .Where(s => s.Category == category)
            .ToListAsync();
            
        return settings.Select(s => s.ToDto());
    }
    
    public async Task<SystemSettingDto> CreateSettingAsync(CreateSystemSettingDto settingDto)
    {
        // Check if key already exists
        var existingSetting = await _dbContext.SystemSettings.FirstOrDefaultAsync(s => s.Key == settingDto.Key);
        if (existingSetting != null)
            throw new InvalidOperationException($"Setting with key {settingDto.Key} already exists");
            
        var setting = settingDto.ToEntity();
        
        await _dbContext.SystemSettings.AddAsync(setting);
        await _dbContext.SaveChangesAsync();
        
        return setting.ToDto();
    }
    
    public async Task<SystemSettingDto> UpdateSettingAsync(Guid id, UpdateSystemSettingDto settingDto)
    {
        var setting = await _dbContext.SystemSettings.FindAsync(id);
        if (setting == null)
            return null;
            
        setting.UpdateFromDto(settingDto);
        
        _dbContext.SystemSettings.Update(setting);
        await _dbContext.SaveChangesAsync();
        
        return setting.ToDto();
    }
    
    public async Task<bool> DeleteSettingAsync(Guid id)
    {
        var setting = await _dbContext.SystemSettings.FindAsync(id);
        if (setting == null)
            return false;
            
        // Soft delete
        setting.IsDeleted = true;
        setting.UpdatedAt = DateTime.UtcNow;
        
        _dbContext.SystemSettings.Update(setting);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
} 