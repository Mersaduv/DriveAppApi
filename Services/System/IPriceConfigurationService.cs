using DriveApp.DTOs.System;
using DriveApp.Enums;

namespace DriveApp.Services.System;

public interface IPriceConfigurationService
{
    Task<PriceConfigurationDto> GetPriceConfigurationByIdAsync(Guid id);
    Task<IEnumerable<PriceConfigurationDto>> GetAllPriceConfigurationsAsync();
    Task<PriceConfigurationDto> GetPriceConfigurationByVehicleTypeAsync(VehicleType vehicleType);
    Task<PriceConfigurationDto> CreatePriceConfigurationAsync(CreatePriceConfigurationDto configDto);
    Task<PriceConfigurationDto> UpdatePriceConfigurationAsync(Guid id, UpdatePriceConfigurationDto configDto);
    Task<bool> DeletePriceConfigurationAsync(Guid id);
    Task<decimal> CalculateTripPriceAsync(VehicleType vehicleType, decimal distance, int durationMinutes);
} 