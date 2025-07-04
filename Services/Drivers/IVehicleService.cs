using DriveApp.DTOs.Drivers;

namespace DriveApp.Services.Drivers;

public interface IVehicleService
{
    Task<VehicleDto> GetVehicleByIdAsync(Guid id);
    Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
    Task<IEnumerable<VehicleDto>> GetVehiclesByDriverIdAsync(Guid driverId);
    Task<VehicleDto> CreateVehicleAsync(Guid driverId, CreateVehicleDto vehicleDto);
    Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleDto vehicleDto);
    Task<bool> DeleteVehicleAsync(Guid id);
    Task<bool> VerifyVehicleAsync(Guid id);
} 