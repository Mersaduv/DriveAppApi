using DriveApp.DTOs.Drivers;
using DriveApp.Enums;

namespace DriveApp.Services.Drivers;

public interface IDriverService
{
    Task<DriverDto> GetDriverByIdAsync(Guid id);
    Task<IEnumerable<DriverDto>> GetAllDriversAsync();
    Task<IEnumerable<DriverDto>> GetDriversByStatusAsync(DriverStatus status);
    Task<DriverDto> CreateDriverAsync(DriverRegistrationDto driverDto);
    Task<DriverDto> UpdateDriverStatusAsync(Guid id, UpdateDriverStatusDto statusDto);
    Task<bool> DeleteDriverAsync(Guid id);
    Task<DriverDto> UpdateDriverLocationAsync(Guid id, DriverLocationUpdateDto locationDto);
    Task<IEnumerable<DriverDto>> GetNearbyDriversAsync(double latitude, double longitude, double radiusKm);
    Task<DriverDto> GetDriverByUserIdAsync(Guid userId);
    Task<bool> ToggleDriverOnlineStatusAsync(Guid id, bool isOnline);
} 