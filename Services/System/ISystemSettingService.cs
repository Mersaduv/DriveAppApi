using DriveApp.DTOs.System;

namespace DriveApp.Services.System;

public interface ISystemSettingService
{
    Task<SystemSettingDto> GetSettingByIdAsync(Guid id);
    Task<SystemSettingDto?> GetSettingByKeyAsync(string key);
    Task<IEnumerable<SystemSettingDto>> GetAllSettingsAsync();
    Task<IEnumerable<SystemSettingDto>> GetPublicSettingsAsync();
    Task<IEnumerable<SystemSettingDto>> GetSettingsByCategoryAsync(string category);
    Task<SystemSettingDto> CreateSettingAsync(CreateSystemSettingDto settingDto);
    Task<SystemSettingDto> UpdateSettingAsync(Guid id, UpdateSystemSettingDto settingDto);
    Task<bool> DeleteSettingAsync(Guid id);
} 