using DriveApp.DTOs.Trips;
using DriveApp.Enums;

namespace DriveApp.Services.Trips;

public interface ITripService
{
    // Get trips
    Task<TripDto> GetTripByIdAsync(Guid id);
    Task<IEnumerable<TripDto>> GetAllTripsAsync(
        TripStatus? status = null,
        Guid? passengerId = null,
        Guid? driverId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 10);
    Task<IEnumerable<TripDto>> GetTripsByStatusAsync(TripStatus status);
    Task<IEnumerable<TripDto>> GetTripsByPassengerIdAsync(
        Guid passengerId,
        TripStatus? status = null,
        int page = 1,
        int pageSize = 10);
    Task<IEnumerable<TripDto>> GetTripsByDriverIdAsync(
        Guid driverId,
        TripStatus? status = null,
        int page = 1,
        int pageSize = 10);

    // Create and manage trips
    Task<TripDto> CreateTripAsync(CreateTripDto tripDto);
    Task<TripDto> RequestTripAsync(Guid passengerId, RequestTripDto tripDto);
    Task<TripDto> UpdateTripStatusAsync(Guid id, UpdateTripStatusDto statusDto);
    Task<TripDto> AcceptTripAsync(Guid tripId, Guid driverId, Guid vehicleId);
    Task<TripDto> DriverArrivedAsync(Guid tripId);
    Task<TripDto> StartTripAsync(Guid tripId);
    Task<TripDto> CompleteTripAsync(Guid tripId, double finalLatitude, double finalLongitude);
    Task<TripDto> CancelTripAsync(Guid tripId, string cancelledBy, string? reason);
    Task<TripDto> UpdateTripLocationAsync(Guid tripId, double latitude, double longitude);
    
    // Trip locations
    Task<TripLocationDto> AddTripLocationAsync(Guid tripId, CreateTripLocationDto locationDto);
    Task<IEnumerable<TripLocationDto>> GetTripLocationsAsync(Guid tripId);
    
    // Trip ratings
    Task<TripRatingDto> AddTripRatingAsync(Guid tripId, CreateTripRatingDto ratingDto);
    Task<TripRatingDto> GetTripRatingAsync(Guid tripId);
    Task<TripRatingDto> RateTripByPassengerAsync(Guid tripId, SubmitPassengerRatingDto ratingDto);
    Task<TripRatingDto> RateTripByDriverAsync(Guid tripId, SubmitDriverRatingDto ratingDto);
    
    // Trip payments
    Task<TripPaymentDto> AddTripPaymentAsync(Guid tripId, CreateTripPaymentDto paymentDto);
    Task<TripPaymentDto> GetTripPaymentAsync(Guid tripId);
    Task<TripPaymentDto> UpdateTripPaymentStatusAsync(Guid tripId, bool isPaid, string? paymentReference);
    Task<TripPaymentDto> ProcessTripPaymentAsync(Guid tripId, ProcessPaymentDto paymentDto);
    
    // Price calculation
    Task<decimal> CalculateTripPriceAsync(Guid tripId);
} 