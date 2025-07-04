using DriveApp.Data;
using DriveApp.DTOs.Trips;
using DriveApp.Enums;
using DriveApp.Models.Trips;
using DriveApp.Models.WebSocket;
using DriveApp.Services.Helpers;
using DriveApp.Services.System;
using Microsoft.EntityFrameworkCore;

namespace DriveApp.Services.Trips;

public class TripService : BaseService, ITripService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPriceConfigurationService _priceConfigurationService;
    private readonly IWebSocketService _webSocketService;
    
    public TripService(
        AppDbContext dbContext, 
        ILogger<TripService> logger,
        IHttpContextAccessor httpContextAccessor,
        IPriceConfigurationService priceConfigurationService,
        IWebSocketService webSocketService) : base(dbContext, logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _priceConfigurationService = priceConfigurationService;
        _webSocketService = webSocketService;
    }
    
    public async Task<TripDto> GetTripByIdAsync(Guid id)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Passenger).ThenInclude(p => p.User)
            .Include(t => t.Driver).ThenInclude(d => d!.User)
            .Include(t => t.Vehicle)
            .Include(t => t.Rating)
            .Include(t => t.Payment)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {id} not found");
        }
        
        return trip.ToDto();
    }
    
    public async Task<IEnumerable<TripDto>> GetAllTripsAsync()
    {
        var trips = await _dbContext.Trips
            .Include(t => t.Passenger).ThenInclude(p => p.User)
            .Include(t => t.Driver).ThenInclude(d => d!.User)
            .Include(t => t.Vehicle)
            .Include(t => t.Rating)
            .Include(t => t.Payment)
            .Where(t => !t.IsDeleted)
            .OrderByDescending(t => t.RequestedAt)
            .ToListAsync();
            
        return trips.Select(t => t.ToDto());
    }
    
    public async Task<IEnumerable<TripDto>> GetTripsByStatusAsync(TripStatus status)
    {
        var trips = await _dbContext.Trips
            .Include(t => t.Passenger).ThenInclude(p => p.User)
            .Include(t => t.Driver).ThenInclude(d => d!.User)
            .Include(t => t.Vehicle)
            .Where(t => t.Status == status && !t.IsDeleted)
            .OrderByDescending(t => t.RequestedAt)
            .ToListAsync();
            
        return trips.Select(t => t.ToDto());
    }
    
    public async Task<IEnumerable<TripDto>> GetTripsByPassengerIdAsync(Guid passengerId)
    {
        // Check if passenger exists
        var passengerExists = await _dbContext.Passengers
            .AnyAsync(p => p.Id == passengerId && !p.IsDeleted);
            
        if (!passengerExists)
        {
            throw new KeyNotFoundException($"Passenger with ID {passengerId} not found");
        }
        
        var trips = await _dbContext.Trips
            .Include(t => t.Driver).ThenInclude(d => d!.User)
            .Include(t => t.Vehicle)
            .Include(t => t.Rating)
            .Include(t => t.Payment)
            .Where(t => t.PassengerId == passengerId && !t.IsDeleted)
            .OrderByDescending(t => t.RequestedAt)
            .ToListAsync();
            
        return trips.Select(t => t.ToDto());
    }
    
    public async Task<IEnumerable<TripDto>> GetTripsByDriverIdAsync(Guid driverId)
    {
        // Check if driver exists
        var driverExists = await _dbContext.Drivers
            .AnyAsync(d => d.Id == driverId && !d.IsDeleted);
            
        if (!driverExists)
        {
            throw new KeyNotFoundException($"Driver with ID {driverId} not found");
        }
        
        var trips = await _dbContext.Trips
            .Include(t => t.Passenger).ThenInclude(p => p.User)
            .Include(t => t.Vehicle)
            .Include(t => t.Rating)
            .Include(t => t.Payment)
            .Where(t => t.DriverId == driverId && !t.IsDeleted)
            .OrderByDescending(t => t.RequestedAt)
            .ToListAsync();
            
        return trips.Select(t => t.ToDto());
    }
    
    public async Task<TripDto> RequestTripAsync(Guid passengerId, RequestTripDto tripDto)
    {
        // Check if passenger exists
        var passenger = await _dbContext.Passengers
            .FirstOrDefaultAsync(p => p.Id == passengerId && !p.IsDeleted);
            
        if (passenger == null)
        {
            throw new KeyNotFoundException($"Passenger with ID {passengerId} not found");
        }
        
        // Check if passenger has any active trips
        var hasActiveTrip = await _dbContext.Trips
            .AnyAsync(t => t.PassengerId == passengerId && 
                     (t.Status == TripStatus.Requested || 
                      t.Status == TripStatus.Accepted || 
                      t.Status == TripStatus.DriverArrived || 
                      t.Status == TripStatus.InProgress) &&
                     !t.IsDeleted);
                     
        if (hasActiveTrip)
        {
            throw new InvalidOperationException("Passenger already has an active trip");
        }
        
        // Generate trip code (unique identifier for the trip)
        var tripCode = $"T{DateTime.UtcNow:yyMMddHHmmss}{Guid.NewGuid().ToString().Substring(0, 4)}";
        
        // Calculate estimated price
        var estimatedPrice = await CalculateEstimatedPriceAsync(
            tripDto.OriginLatitude, 
            tripDto.OriginLongitude, 
            tripDto.DestinationLatitude, 
            tripDto.DestinationLongitude,
            tripDto.RequestedVehicleType
        );
        
        // Create trip entity
        var trip = new Trip
        {
            PassengerId = passengerId,
            TripCode = tripCode,
            OriginAddress = tripDto.OriginAddress,
            OriginLatitude = tripDto.OriginLatitude,
            OriginLongitude = tripDto.OriginLongitude,
            DestinationAddress = tripDto.DestinationAddress,
            DestinationLatitude = tripDto.DestinationLatitude,
            DestinationLongitude = tripDto.DestinationLongitude,
            Status = TripStatus.Requested,
            RequestedVehicleType = tripDto.RequestedVehicleType,
            EstimatedPrice = estimatedPrice,
            RequestedAt = DateTime.UtcNow,
            PassengerNotes = tripDto.PassengerNotes,
            CreatedBy = GetCurrentUserName()
        };
        
        await _dbContext.Trips.AddAsync(trip);
        await _dbContext.SaveChangesAsync();
        
        // Notify available drivers about the new trip request via WebSocket
        await NotifyAvailableDriversAsync(trip);
        
        var createdTrip = await _dbContext.Trips
            .Include(t => t.Passenger).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(t => t.Id == trip.Id);
            
        return createdTrip!.ToDto();
    }
    
    public async Task<TripDto> UpdateTripStatusAsync(Guid id, UpdateTripStatusDto statusDto)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Passenger)
            .Include(t => t.Driver)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {id} not found");
        }
        
        // Update status
        trip.Status = statusDto.Status;
        
        // Handle status-specific updates
        switch (statusDto.Status)
        {
            case TripStatus.Cancelled:
                trip.CancelledAt = DateTime.UtcNow;
                trip.CancellationReason = statusDto.CancellationReason;
                trip.CancelledBy = GetCurrentUserName(); // Should be either "Passenger" or "Driver"
                break;
                
            case TripStatus.Completed:
                trip.CompletedAt = DateTime.UtcNow;
                
                // Update final price and actual duration
                trip.FinalPrice = await CalculateTripPriceAsync(trip.Id);
                if (trip.StartedAt.HasValue)
                {
                    var duration = (int)(DateTime.UtcNow - trip.StartedAt.Value).TotalMinutes;
                    trip.ActualDuration = duration;
                }
                
                // Update passenger and driver trip counts
                var passenger = trip.Passenger;
                passenger.TotalTrips += 1;
                
                if (trip.Driver != null)
                {
                    var driver = trip.Driver;
                    driver.TotalTrips += 1;
                }
                
                break;
        }
        
        trip.UpdatedAt = DateTime.UtcNow;
        trip.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        // Notify via WebSocket about the status change
        await NotifyTripStatusChangeAsync(trip);
        
        return trip.ToDto();
    }
    
    public async Task<TripDto> AcceptTripAsync(Guid tripId, Guid driverId)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Passenger)
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        if (trip.Status != TripStatus.Requested)
        {
            throw new InvalidOperationException($"Trip status is {trip.Status}, only Requested trips can be accepted");
        }
        
        // Check if driver exists and is available
        var driver = await _dbContext.Drivers
            .Include(d => d.Vehicles)
            .FirstOrDefaultAsync(d => d.Id == driverId && 
                               d.Status == DriverStatus.Approved && 
                               d.IsOnline && 
                               !d.IsDeleted);
                               
        if (driver == null)
        {
            throw new InvalidOperationException("Driver not found or not available");
        }
        
        // Check if driver has an active trip
        var hasActiveTrip = await _dbContext.Trips
            .AnyAsync(t => t.DriverId == driverId && 
                     (t.Status == TripStatus.Accepted || 
                      t.Status == TripStatus.DriverArrived || 
                      t.Status == TripStatus.InProgress) &&
                     !t.IsDeleted);
                     
        if (hasActiveTrip)
        {
            throw new InvalidOperationException("Driver already has an active trip");
        }
        
        // Find a suitable vehicle for the trip
        var vehicle = driver.Vehicles
            .FirstOrDefault(v => v.VehicleType == trip.RequestedVehicleType && 
                              v.IsActive && v.IsVerified && !v.IsDeleted);
                              
        if (vehicle == null)
        {
            // If no exact match, try to find any vehicle
            vehicle = driver.Vehicles
                .FirstOrDefault(v => v.IsActive && v.IsVerified && !v.IsDeleted);
                
            if (vehicle == null)
            {
                throw new InvalidOperationException("Driver has no suitable vehicle for this trip");
            }
        }
        
        // Update trip
        trip.DriverId = driverId;
        trip.VehicleId = vehicle.Id;
        trip.Status = TripStatus.Accepted;
        trip.AcceptedAt = DateTime.UtcNow;
        trip.UpdatedAt = DateTime.UtcNow;
        trip.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        // Notify via WebSocket
        await NotifyTripStatusChangeAsync(trip);
        
        var updatedTrip = await _dbContext.Trips
            .Include(t => t.Passenger).ThenInclude(p => p.User)
            .Include(t => t.Driver).ThenInclude(d => d!.User)
            .Include(t => t.Vehicle)
            .FirstOrDefaultAsync(t => t.Id == tripId);
            
        return updatedTrip!.ToDto();
    }
    
    public async Task<TripDto> DriverArrivedAsync(Guid tripId)
    {
        var trip = await _dbContext.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        if (trip.Status != TripStatus.Accepted)
        {
            throw new InvalidOperationException($"Trip status is {trip.Status}, only Accepted trips can be marked as DriverArrived");
        }
        
        // Update trip
        trip.Status = TripStatus.DriverArrived;
        trip.DriverArrivedAt = DateTime.UtcNow;
        trip.UpdatedAt = DateTime.UtcNow;
        trip.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        // Notify via WebSocket
        await NotifyTripStatusChangeAsync(trip);
        
        return trip.ToDto();
    }
    
    public async Task<TripDto> StartTripAsync(Guid tripId)
    {
        var trip = await _dbContext.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        if (trip.Status != TripStatus.DriverArrived)
        {
            throw new InvalidOperationException($"Trip status is {trip.Status}, only DriverArrived trips can be started");
        }
        
        // Update trip
        trip.Status = TripStatus.InProgress;
        trip.StartedAt = DateTime.UtcNow;
        trip.UpdatedAt = DateTime.UtcNow;
        trip.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        // Notify via WebSocket
        await NotifyTripStatusChangeAsync(trip);
        
        return trip.ToDto();
    }
    
    public async Task<TripDto> CompleteTripAsync(Guid tripId)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Passenger)
            .Include(t => t.Driver)
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        if (trip.Status != TripStatus.InProgress)
        {
            throw new InvalidOperationException($"Trip status is {trip.Status}, only InProgress trips can be completed");
        }
        
        // Update trip
        trip.Status = TripStatus.Completed;
        trip.CompletedAt = DateTime.UtcNow;
        
        // Calculate trip duration
        if (trip.StartedAt.HasValue)
        {
            var duration = (int)(DateTime.UtcNow - trip.StartedAt.Value).TotalMinutes;
            trip.ActualDuration = duration;
        }
        
        // Calculate final price
        trip.FinalPrice = await CalculateTripPriceAsync(trip.Id);
        
        // Update passenger and driver trip counts
        if (trip.Passenger != null)
        {
            var passenger = trip.Passenger;
            passenger.TotalTrips += 1;
        }
        
        if (trip.Driver != null)
        {
            var driver = trip.Driver;
            driver.TotalTrips += 1;
        }
        
        trip.UpdatedAt = DateTime.UtcNow;
        trip.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        // Notify via WebSocket
        await NotifyTripStatusChangeAsync(trip);
        
        return trip.ToDto();
    }
    
    public async Task<TripDto> CancelTripAsync(Guid tripId, string? reason, string cancelledBy)
    {
        var trip = await _dbContext.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        // Only non-completed and non-cancelled trips can be cancelled
        if (trip.Status == TripStatus.Completed || trip.Status == TripStatus.Cancelled)
        {
            throw new InvalidOperationException($"Trip with status {trip.Status} cannot be cancelled");
        }
        
        // Update trip
        trip.Status = TripStatus.Cancelled;
        trip.CancelledAt = DateTime.UtcNow;
        trip.CancellationReason = reason;
        trip.CancelledBy = cancelledBy; // "Passenger", "Driver", or "System"
        trip.UpdatedAt = DateTime.UtcNow;
        trip.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        // Notify via WebSocket
        await NotifyTripStatusChangeAsync(trip);
        
        return trip.ToDto();
    }
    
    public async Task<TripDto> UpdateTripLocationAsync(Guid tripId, double latitude, double longitude)
    {
        var trip = await _dbContext.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        // Only add location updates for trips that are in progress
        if (trip.Status != TripStatus.InProgress && trip.Status != TripStatus.Accepted && trip.Status != TripStatus.DriverArrived)
        {
            throw new InvalidOperationException($"Cannot update location for trip with status {trip.Status}");
        }
        
        // Add to location history
        var location = new TripLocation
        {
            TripId = tripId,
            Latitude = latitude,
            Longitude = longitude,
            Timestamp = DateTime.UtcNow
        };
        
        await _dbContext.TripLocations.AddAsync(location);
        await _dbContext.SaveChangesAsync();
        
        // Notify via WebSocket (location update)
        await NotifyTripLocationUpdateAsync(trip, latitude, longitude);
        
        return trip.ToDto();
    }
    
    public async Task<TripRatingDto> RateTripByPassengerAsync(Guid tripId, SubmitPassengerRatingDto ratingDto)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Rating)
            .Include(t => t.Driver)
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        if (trip.Status != TripStatus.Completed)
        {
            throw new InvalidOperationException("Only completed trips can be rated");
        }
        
        // Check rating value
        if (ratingDto.Rating < 1 || ratingDto.Rating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(ratingDto.Rating), "Rating must be between 1 and 5");
        }
        
        TripRating rating;
        if (trip.Rating == null)
        {
            // Create new rating
            rating = new TripRating
            {
                TripId = tripId,
                PassengerRating = ratingDto.Rating,
                PassengerComment = ratingDto.Comment,
                RatedAt = DateTime.UtcNow
            };
            
            await _dbContext.TripRatings.AddAsync(rating);
        }
        else
        {
            // Update existing rating
            rating = trip.Rating;
            rating.PassengerRating = ratingDto.Rating;
            rating.PassengerComment = ratingDto.Comment;
            rating.RatedAt = DateTime.UtcNow;
            rating.UpdatedAt = DateTime.UtcNow;
            rating.UpdatedBy = GetCurrentUserName();
        }
        
        // Update driver's average rating
        if (trip.Driver != null)
        {
            var driver = trip.Driver;
            var driverRatings = await _dbContext.TripRatings
                .Where(r => r.Trip!.DriverId == driver.Id && r.PassengerRating > 0)
                .ToListAsync();
                
            if (driverRatings.Any())
            {
                driver.Rating = (decimal)driverRatings.Average(r => r.PassengerRating);
            }
        }
        
        await _dbContext.SaveChangesAsync();
        
        return rating.ToDto();
    }
    
    public async Task<TripRatingDto> RateTripByDriverAsync(Guid tripId, SubmitDriverRatingDto ratingDto)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Rating)
            .Include(t => t.Passenger)
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        if (trip.Status != TripStatus.Completed)
        {
            throw new InvalidOperationException("Only completed trips can be rated");
        }
        
        // Check rating value
        if (ratingDto.Rating < 1 || ratingDto.Rating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(ratingDto.Rating), "Rating must be between 1 and 5");
        }
        
        TripRating rating;
        if (trip.Rating == null)
        {
            // Create new rating
            rating = new TripRating
            {
                TripId = tripId,
                DriverRating = ratingDto.Rating,
                DriverComment = ratingDto.Comment,
                RatedAt = DateTime.UtcNow
            };
            
            await _dbContext.TripRatings.AddAsync(rating);
        }
        else
        {
            // Update existing rating
            rating = trip.Rating;
            rating.DriverRating = ratingDto.Rating;
            rating.DriverComment = ratingDto.Comment;
            rating.UpdatedAt = DateTime.UtcNow;
            rating.UpdatedBy = GetCurrentUserName();
        }
        
        // Update passenger's average rating
        if (trip.Passenger != null)
        {
            var passenger = trip.Passenger;
            var passengerRatings = await _dbContext.TripRatings
                .Where(r => r.Trip!.PassengerId == passenger.Id && r.DriverRating.HasValue)
                .ToListAsync();
                
            if (passengerRatings.Any())
            {
                passenger.Rating = (decimal)passengerRatings.Average(r => r.DriverRating!.Value);
            }
        }
        
        await _dbContext.SaveChangesAsync();
        
        return rating.ToDto();
    }
    
    public async Task<TripPaymentDto> ProcessTripPaymentAsync(Guid tripId, ProcessPaymentDto paymentDto)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Payment)
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        if (trip.Status != TripStatus.Completed)
        {
            throw new InvalidOperationException("Only completed trips can be paid");
        }
        
        if (trip.FinalPrice == null)
        {
            trip.FinalPrice = await CalculateTripPriceAsync(trip.Id);
        }
        
        var amount = trip.FinalPrice!.Value;
        
        TripPayment payment;
        if (trip.Payment == null)
        {
            // Create new payment
            payment = new TripPayment
            {
                TripId = tripId,
                Amount = amount,
                PaymentMethod = paymentDto.PaymentMethod,
                PaymentReference = paymentDto.PaymentReference,
                IsPaid = true,
                PaidAt = DateTime.UtcNow
            };
            
            await _dbContext.TripPayments.AddAsync(payment);
        }
        else
        {
            // Update existing payment
            payment = trip.Payment;
            payment.Amount = amount;
            payment.PaymentMethod = paymentDto.PaymentMethod;
            payment.PaymentReference = paymentDto.PaymentReference;
            payment.IsPaid = true;
            payment.PaidAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.UpdatedBy = GetCurrentUserName();
        }
        
        await _dbContext.SaveChangesAsync();
        
        return payment.ToDto();
    }
    
    public async Task<decimal> CalculateTripPriceAsync(Guid tripId)
    {
        var trip = await _dbContext.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        // For completed trips, use actual duration; otherwise, use estimated
        var duration = trip.ActualDuration ?? trip.EstimatedDuration ?? 0;
        
        // Get price configuration for the vehicle type
        var priceConfig = await _priceConfigurationService.GetPriceConfigurationByVehicleTypeAsync(trip.RequestedVehicleType);
        
        if (priceConfig == null)
        {
            throw new InvalidOperationException($"Price configuration not found for vehicle type {trip.RequestedVehicleType}");
        }
        
        // Calculate distance if not already set
        decimal? tripDistance = trip.Distance;
        if (!tripDistance.HasValue)
        {
            double distanceDouble = CalculateDistanceInKilometers(
                trip.OriginLatitude,
                trip.OriginLongitude,
                trip.DestinationLatitude,
                trip.DestinationLongitude
            );
            tripDistance = Convert.ToDecimal(distanceDouble);
        }
        
        // Use the price configuration service for calculation
        return await _priceConfigurationService.CalculateTripPriceAsync(
            trip.RequestedVehicleType,
            tripDistance.Value,
            duration
        );
    }
    
    #region Private Helper Methods
    
    private async Task<decimal> CalculateEstimatedPriceAsync(
        double originLat, double originLong, 
        double destLat, double destLong, 
        VehicleType vehicleType)
    {
        // Calculate estimated distance
        double distanceInKm = CalculateDistanceInKilometers(originLat, originLong, destLat, destLong);
        decimal distance = Convert.ToDecimal(distanceInKm);
        
        // Estimate duration (assume average speed of 40 km/h)
        var estimatedDuration = (int)(distance / 40.0M * 60M); // in minutes
        
        // Use the price configuration service for calculation
        return await _priceConfigurationService.CalculateTripPriceAsync(
            vehicleType,
            distance,
            estimatedDuration
        );
    }
    
    private double CalculateDistanceInKilometers(double lat1, double lon1, double lat2, double lon2)
    {
        // Using the Haversine formula for distance calculation
        const double earthRadiusKm = 6371.0;
        
        var dLat = (lat2 - lat1) * (Math.PI / 180);
        var dLon = (lon2 - lon1) * (Math.PI / 180);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
               Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) *
               Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = earthRadiusKm * c;
        
        return distance;
    }
    
    private async Task NotifyAvailableDriversAsync(Trip trip)
    {
        try
        {
            // In a real implementation, this would find nearby drivers and send notifications
            // For now, just broadcast to all connected clients
            await _webSocketService.SendMessageToAllAsync("trip_request", new
            {
                TripId = trip.Id,
                TripCode = trip.TripCode,
                OriginAddress = trip.OriginAddress,
                DestinationAddress = trip.DestinationAddress,
                RequestedVehicleType = trip.RequestedVehicleType,
                EstimatedPrice = trip.EstimatedPrice
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying drivers about trip request");
        }
    }
    
    private async Task NotifyTripStatusChangeAsync(Trip trip)
    {
        try
        {
            // Notify both the driver and passenger about trip status change
            var tripUpdate = new TripUpdate
            {
                TripId = trip.Id,
                Status = trip.Status,
                Message = $"Trip status updated to {trip.Status}",
                AdditionalData = new
                {
                    TripCode = trip.TripCode,
                    DriverId = trip.DriverId,
                    PassengerId = trip.PassengerId
                }
            };
            
            await _webSocketService.SendTripUpdateAsync(tripUpdate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying about trip status change");
        }
    }
    
    private async Task NotifyTripLocationUpdateAsync(Trip trip, double latitude, double longitude)
    {
        try
        {
            // Notify the passenger about driver location
            if (trip.PassengerId != Guid.Empty)
            {
                await _webSocketService.SendMessageToUserAsync(trip.PassengerId.ToString(), "trip_location_update", new
                {
                    TripId = trip.Id,
                    TripCode = trip.TripCode,
                    Latitude = latitude,
                    Longitude = longitude,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying about trip location update");
        }
    }
    
    private string GetCurrentUserName()
    {
        if (_httpContextAccessor.HttpContext?.User?.Identity?.Name != null)
        {
            return _httpContextAccessor.HttpContext.User.Identity.Name;
        }
        
        return "System";
    }

    public async Task<IEnumerable<TripDto>> GetAllTripsAsync(TripStatus? status = null, Guid? passengerId = null, Guid? driverId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10)
    {
        IQueryable<Trip> query = _dbContext.Trips
            .Include(t => t.Passenger).ThenInclude(p => p.User)
            .Include(t => t.Driver).ThenInclude(d => d!.User)
            .Include(t => t.Vehicle)
            .Include(t => t.Rating)
            .Include(t => t.Payment)
            .Where(t => !t.IsDeleted);
            
        // Apply filters
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
            
        if (passengerId.HasValue)
            query = query.Where(t => t.PassengerId == passengerId.Value);
            
        if (driverId.HasValue)
            query = query.Where(t => t.DriverId == driverId.Value);
            
        if (startDate.HasValue)
            query = query.Where(t => t.RequestedAt >= startDate.Value);
            
        if (endDate.HasValue)
            query = query.Where(t => t.RequestedAt <= endDate.Value);
        
        // Apply pagination
        query = query.OrderByDescending(t => t.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
            
        var trips = await query.ToListAsync();
        return trips.Select(t => t.ToDto());
    }

    public async Task<IEnumerable<TripDto>> GetTripsByPassengerIdAsync(Guid passengerId, TripStatus? status = null, int page = 1, int pageSize = 10)
    {
        // Check if passenger exists
        var passengerExists = await _dbContext.Passengers
            .AnyAsync(p => p.Id == passengerId && !p.IsDeleted);
            
        if (!passengerExists)
        {
            throw new KeyNotFoundException($"Passenger with ID {passengerId} not found");
        }
        
        IQueryable<Trip> query = _dbContext.Trips
            .Include(t => t.Driver).ThenInclude(d => d!.User)
            .Include(t => t.Vehicle)
            .Include(t => t.Rating)
            .Include(t => t.Payment)
            .Where(t => t.PassengerId == passengerId && !t.IsDeleted);
            
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
            
        // Apply pagination
        query = query.OrderByDescending(t => t.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
            
        var trips = await query.ToListAsync();
        return trips.Select(t => t.ToDto());
    }

    public async Task<IEnumerable<TripDto>> GetTripsByDriverIdAsync(Guid driverId, TripStatus? status = null, int page = 1, int pageSize = 10)
    {
        // Check if driver exists
        var driverExists = await _dbContext.Drivers
            .AnyAsync(d => d.Id == driverId && !d.IsDeleted);
            
        if (!driverExists)
        {
            throw new KeyNotFoundException($"Driver with ID {driverId} not found");
        }
        
        IQueryable<Trip> query = _dbContext.Trips
            .Include(t => t.Passenger).ThenInclude(p => p.User)
            .Include(t => t.Vehicle)
            .Include(t => t.Rating)
            .Include(t => t.Payment)
            .Where(t => t.DriverId == driverId && !t.IsDeleted);
            
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
            
        // Apply pagination
        query = query.OrderByDescending(t => t.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
            
        var trips = await query.ToListAsync();
        return trips.Select(t => t.ToDto());
    }

    public async Task<TripDto> CreateTripAsync(CreateTripDto tripDto)
    {
        // Check if passenger exists
        var passenger = await _dbContext.Passengers
            .FirstOrDefaultAsync(p => p.Id == tripDto.PassengerId && !p.IsDeleted);
            
        if (passenger == null)
        {
            throw new KeyNotFoundException($"Passenger with ID {tripDto.PassengerId} not found");
        }
        
        // Check if passenger has any active trips
        var hasActiveTrip = await _dbContext.Trips
            .AnyAsync(t => t.PassengerId == tripDto.PassengerId && 
                     (t.Status == TripStatus.Requested || 
                      t.Status == TripStatus.Accepted || 
                      t.Status == TripStatus.DriverArrived || 
                      t.Status == TripStatus.InProgress) &&
                     !t.IsDeleted);
                     
        if (hasActiveTrip)
        {
            throw new InvalidOperationException("Passenger already has an active trip");
        }
        
        // Generate trip code
        var tripCode = $"T{DateTime.UtcNow:yyMMddHHmmss}{Guid.NewGuid().ToString().Substring(0, 4)}";
        
        // Calculate estimated price
        var estimatedPrice = await CalculateEstimatedPriceAsync(
            tripDto.OriginLatitude, 
            tripDto.OriginLongitude, 
            tripDto.DestinationLatitude, 
            tripDto.DestinationLongitude,
            tripDto.RequestedVehicleType
        );
        
        // Create trip entity
        var trip = new Trip
        {
            PassengerId = tripDto.PassengerId,
            TripCode = tripCode,
            OriginAddress = tripDto.OriginAddress,
            OriginLatitude = tripDto.OriginLatitude,
            OriginLongitude = tripDto.OriginLongitude,
            DestinationAddress = tripDto.DestinationAddress,
            DestinationLatitude = tripDto.DestinationLatitude,
            DestinationLongitude = tripDto.DestinationLongitude,
            Status = TripStatus.Requested,
            RequestedVehicleType = tripDto.RequestedVehicleType,
            EstimatedPrice = estimatedPrice,
            RequestedAt = DateTime.UtcNow,
            PassengerNotes = tripDto.PassengerNotes,
            CreatedBy = GetCurrentUserName()
        };
        
        await _dbContext.Trips.AddAsync(trip);
        await _dbContext.SaveChangesAsync();
        
        // Notify available drivers about the new trip request
        await NotifyAvailableDriversAsync(trip);
        
        var createdTrip = await _dbContext.Trips
            .Include(t => t.Passenger).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(t => t.Id == trip.Id);
            
        return createdTrip!.ToDto();
    }

    public async Task<TripDto> AcceptTripAsync(Guid tripId, Guid driverId, Guid vehicleId)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Passenger)
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        if (trip.Status != TripStatus.Requested)
        {
            throw new InvalidOperationException($"Trip status is {trip.Status}, only Requested trips can be accepted");
        }
        
        // Check if driver exists and is available
        var driver = await _dbContext.Drivers
            .FirstOrDefaultAsync(d => d.Id == driverId && 
                               d.Status == DriverStatus.Approved && 
                               d.IsOnline && 
                               !d.IsDeleted);
                               
        if (driver == null)
        {
            throw new InvalidOperationException("Driver not found or not available");
        }
        
        // Check if driver has an active trip
        var hasActiveTrip = await _dbContext.Trips
            .AnyAsync(t => t.DriverId == driverId && 
                     (t.Status == TripStatus.Accepted || 
                      t.Status == TripStatus.DriverArrived || 
                      t.Status == TripStatus.InProgress) &&
                     !t.IsDeleted);
                     
        if (hasActiveTrip)
        {
            throw new InvalidOperationException("Driver already has an active trip");
        }
        
        // Verify the vehicle belongs to the driver and is active
        var vehicle = await _dbContext.Vehicles
            .FirstOrDefaultAsync(v => v.Id == vehicleId && v.DriverId == driverId &&
                                v.IsActive && v.IsVerified && !v.IsDeleted);
                                
        if (vehicle == null)
        {
            throw new InvalidOperationException("Vehicle not found or not valid for this driver");
        }
        
        // Update trip
        trip.DriverId = driverId;
        trip.VehicleId = vehicleId;
        trip.Status = TripStatus.Accepted;
        trip.AcceptedAt = DateTime.UtcNow;
        trip.UpdatedAt = DateTime.UtcNow;
        trip.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        // Notify via WebSocket
        await NotifyTripStatusChangeAsync(trip);
        
        var updatedTrip = await _dbContext.Trips
            .Include(t => t.Passenger).ThenInclude(p => p.User)
            .Include(t => t.Driver).ThenInclude(d => d!.User)
            .Include(t => t.Vehicle)
            .FirstOrDefaultAsync(t => t.Id == tripId);
            
        return updatedTrip!.ToDto();
    }

    public async Task<TripDto> CompleteTripAsync(Guid tripId, double finalLatitude, double finalLongitude)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Passenger)
            .Include(t => t.Driver)
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        if (trip.Status != TripStatus.InProgress)
        {
            throw new InvalidOperationException($"Trip status is {trip.Status}, only InProgress trips can be completed");
        }
        
        // Update trip
        trip.Status = TripStatus.Completed;
        trip.CompletedAt = DateTime.UtcNow;
        
        // Add final location to trip locations
        await _dbContext.TripLocations.AddAsync(new Models.Trips.TripLocation
        {
            TripId = tripId,
            Latitude = finalLatitude,
            Longitude = finalLongitude,
            Timestamp = DateTime.UtcNow
        });
        
        // Calculate trip duration
        if (trip.StartedAt.HasValue)
        {
            var duration = (int)(DateTime.UtcNow - trip.StartedAt.Value).TotalMinutes;
            trip.ActualDuration = duration;
        }
        
        // Calculate final price
        trip.FinalPrice = await CalculateTripPriceAsync(trip.Id);
        
        // Update passenger and driver trip counts
        if (trip.Passenger != null)
        {
            var passenger = trip.Passenger;
            passenger.TotalTrips += 1;
        }
        
        if (trip.Driver != null)
        {
            var driver = trip.Driver;
            driver.TotalTrips += 1;
        }
        
        trip.UpdatedAt = DateTime.UtcNow;
        trip.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        // Notify via WebSocket
        await NotifyTripStatusChangeAsync(trip);
        
        return trip.ToDto();
    }

    public async Task<TripLocationDto> AddTripLocationAsync(Guid tripId, CreateTripLocationDto locationDto)
    {
        var trip = await _dbContext.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        // Only add location updates for active trips
        if (trip.Status != TripStatus.InProgress && trip.Status != TripStatus.Accepted && trip.Status != TripStatus.DriverArrived)
        {
            throw new InvalidOperationException($"Cannot add location for trip with status {trip.Status}");
        }
        
        var location = new Models.Trips.TripLocation
        {
            TripId = tripId,
            Latitude = locationDto.Latitude,
            Longitude = locationDto.Longitude,
            Speed = locationDto.Speed,
            Heading = locationDto.Heading,
            Timestamp = DateTime.UtcNow
        };
        
        await _dbContext.TripLocations.AddAsync(location);
        await _dbContext.SaveChangesAsync();
        
        // Notify via WebSocket (location update)
        await NotifyTripLocationUpdateAsync(trip, locationDto.Latitude, locationDto.Longitude);
        
        return new TripLocationDto
        {
            Id = location.Id,
            TripId = location.TripId,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Speed = location.Speed,
            Heading = location.Heading,
            Timestamp = location.Timestamp
        };
    }

    public async Task<IEnumerable<TripLocationDto>> GetTripLocationsAsync(Guid tripId)
    {
        var trip = await _dbContext.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        var locations = await _dbContext.TripLocations
            .Where(l => l.TripId == tripId)
            .OrderBy(l => l.Timestamp)
            .ToListAsync();
            
        return locations.Select(l => new TripLocationDto
        {
            Id = l.Id,
            TripId = l.TripId,
            Latitude = l.Latitude,
            Longitude = l.Longitude,
            Speed = l.Speed,
            Heading = l.Heading,
            Timestamp = l.Timestamp
        });
    }

    public async Task<TripRatingDto> AddTripRatingAsync(Guid tripId, CreateTripRatingDto ratingDto)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Rating)
            .Include(t => t.Passenger)
            .Include(t => t.Driver)
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        if (trip.Status != TripStatus.Completed)
        {
            throw new InvalidOperationException("Only completed trips can be rated");
        }
        
        // Check rating value
        if (ratingDto.Rating < 1 || ratingDto.Rating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(ratingDto.Rating), "Rating must be between 1 and 5");
        }
        
        Models.Trips.TripRating rating;
        
        if (trip.Rating == null)
        {
            // Create new rating
            rating = new Models.Trips.TripRating
            {
                TripId = tripId,
                RatedAt = DateTime.UtcNow
            };
            
            if (ratingDto.IsFromPassenger)
            {
                rating.PassengerRating = ratingDto.Rating;
                rating.PassengerComment = ratingDto.Comment;
                
                // Update driver's rating if applicable
                if (trip.Driver != null)
                {
                    await UpdateDriverAverageRatingAsync(trip.Driver.Id);
                }
            }
            else
            {
                rating.DriverRating = ratingDto.Rating;
                rating.DriverComment = ratingDto.Comment;
                
                // Update passenger's rating
                if (trip.Passenger != null)
                {
                    await UpdatePassengerAverageRatingAsync(trip.Passenger.Id);
                }
            }
            
            await _dbContext.TripRatings.AddAsync(rating);
        }
        else
        {
            // Update existing rating
            rating = trip.Rating;
            
            if (ratingDto.IsFromPassenger)
            {
                rating.PassengerRating = ratingDto.Rating;
                rating.PassengerComment = ratingDto.Comment;
                
                // Update driver's rating
                if (trip.Driver != null)
                {
                    await UpdateDriverAverageRatingAsync(trip.Driver.Id);
                }
            }
            else
            {
                rating.DriverRating = ratingDto.Rating;
                rating.DriverComment = ratingDto.Comment;
                
                // Update passenger's rating
                if (trip.Passenger != null)
                {
                    await UpdatePassengerAverageRatingAsync(trip.Passenger.Id);
                }
            }
            
            rating.UpdatedAt = DateTime.UtcNow;
            rating.UpdatedBy = GetCurrentUserName();
        }
        
        await _dbContext.SaveChangesAsync();
        
        return rating.ToDto();
    }
    
    private async Task UpdateDriverAverageRatingAsync(Guid driverId)
    {
        var driver = await _dbContext.Drivers
            .FirstOrDefaultAsync(d => d.Id == driverId && !d.IsDeleted);
            
        if (driver != null)
        {
            var driverRatings = await _dbContext.TripRatings
                .Where(r => r.Trip!.DriverId == driverId && r.PassengerRating > 0)
                .ToListAsync();
                
            if (driverRatings.Any())
            {
                driver.Rating = (decimal)driverRatings.Average(r => r.PassengerRating);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
    
    private async Task UpdatePassengerAverageRatingAsync(Guid passengerId)
    {
        var passenger = await _dbContext.Passengers
            .FirstOrDefaultAsync(p => p.Id == passengerId && !p.IsDeleted);
            
        if (passenger != null)
        {
            var passengerRatings = await _dbContext.TripRatings
                .Where(r => r.Trip!.PassengerId == passengerId && r.DriverRating.HasValue)
                .ToListAsync();
                
            if (passengerRatings.Any())
            {
                passenger.Rating = (decimal)passengerRatings.Average(r => r.DriverRating!.Value);
                await _dbContext.SaveChangesAsync();
            }
        }
    }

    public async Task<TripRatingDto> GetTripRatingAsync(Guid tripId)
    {
        var trip = await _dbContext.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        var rating = await _dbContext.TripRatings
            .FirstOrDefaultAsync(r => r.TripId == tripId);
            
        if (rating == null)
        {
            throw new KeyNotFoundException($"Rating for trip ID {tripId} not found");
        }
        
        return rating.ToDto();
    }

    public async Task<TripPaymentDto> AddTripPaymentAsync(Guid tripId, CreateTripPaymentDto paymentDto)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Payment)
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        if (trip.Status != TripStatus.Completed)
        {
            throw new InvalidOperationException("Only completed trips can have payments added");
        }
        
        // If payment already exists, throw exception
        if (trip.Payment != null)
        {
            throw new InvalidOperationException($"Payment for trip ID {tripId} already exists");
        }
        
        var payment = new Models.Trips.TripPayment
        {
            TripId = tripId,
            Amount = paymentDto.Amount,
            PaymentMethod = paymentDto.PaymentMethod,
            PaymentReference = paymentDto.PaymentReference,
            IsPaid = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = GetCurrentUserName()
        };
        
        await _dbContext.TripPayments.AddAsync(payment);
        await _dbContext.SaveChangesAsync();
        
        return payment.ToDto();
    }

    public async Task<TripPaymentDto> GetTripPaymentAsync(Guid tripId)
    {
        var trip = await _dbContext.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        var payment = await _dbContext.TripPayments
            .FirstOrDefaultAsync(p => p.TripId == tripId);
            
        if (payment == null)
        {
            throw new KeyNotFoundException($"Payment for trip ID {tripId} not found");
        }
        
        return payment.ToDto();
    }

    public async Task<TripPaymentDto> UpdateTripPaymentStatusAsync(Guid tripId, bool isPaid, string? paymentReference)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Payment)
            .FirstOrDefaultAsync(t => t.Id == tripId && !t.IsDeleted);
            
        if (trip == null)
        {
            throw new KeyNotFoundException($"Trip with ID {tripId} not found");
        }
        
        if (trip.Payment == null)
        {
            throw new KeyNotFoundException($"Payment for trip ID {tripId} not found");
        }
        
        var payment = trip.Payment;
        payment.IsPaid = isPaid;
        
        if (isPaid && !payment.PaidAt.HasValue)
        {
            payment.PaidAt = DateTime.UtcNow;
        }
        
        if (paymentReference != null)
        {
            payment.PaymentReference = paymentReference;
        }
        
        payment.UpdatedAt = DateTime.UtcNow;
        payment.UpdatedBy = GetCurrentUserName();
        
        await _dbContext.SaveChangesAsync();
        
        return payment.ToDto();
    }

    #endregion
}