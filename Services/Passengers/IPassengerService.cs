using DriveApp.DTOs.Passengers;

namespace DriveApp.Services.Passengers;

public interface IPassengerService
{
    Task<PassengerDto> GetPassengerByIdAsync(Guid id);
    Task<IEnumerable<PassengerDto>> GetAllPassengersAsync();
    Task<PassengerDto> CreatePassengerAsync(PassengerRegistrationDto passengerDto);
    Task<PassengerDto> UpdatePassengerAsync(Guid id, UpdatePassengerDto passengerDto);
    Task<bool> DeletePassengerAsync(Guid id);
    Task<PassengerDto> GetPassengerByUserIdAsync(Guid userId);
    Task<IEnumerable<PassengerFavoriteLocationDto>> GetFavoriteLocationsAsync(Guid passengerId);
    Task<PassengerFavoriteLocationDto> AddFavoriteLocationAsync(Guid passengerId, CreateFavoriteLocationDto locationDto);
    Task<PassengerFavoriteLocationDto> UpdateFavoriteLocationAsync(Guid locationId, UpdateFavoriteLocationDto locationDto);
    Task<bool> DeleteFavoriteLocationAsync(Guid locationId);
} 