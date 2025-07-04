using DriveApp.Enums;
using DriveApp.DTOs.Drivers;
using DriveApp.DTOs.Passengers;

namespace DriveApp.DTOs.Trips;

public class TripDto
{
    public Guid Id { get; set; }
    public Guid PassengerId { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? VehicleId { get; set; }
    public string TripCode { get; set; } = string.Empty;
    
    // Origin Information
    public string OriginAddress { get; set; } = string.Empty;
    public double OriginLatitude { get; set; }
    public double OriginLongitude { get; set; }
    
    // Destination Information
    public string DestinationAddress { get; set; } = string.Empty;
    public double DestinationLatitude { get; set; }
    public double DestinationLongitude { get; set; }
    
    // Trip Details
    public TripStatus Status { get; set; }
    public VehicleType RequestedVehicleType { get; set; }
    public decimal EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal? Distance { get; set; }
    public int? EstimatedDuration { get; set; }
    public int? ActualDuration { get; set; }
    
    // Timing
    public DateTime RequestedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DriverArrivedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    // Additional Info
    public string? PassengerNotes { get; set; }
    public string? CancellationReason { get; set; }
    public string? CancelledBy { get; set; }
    
    // Navigation Properties
    public PassengerDto? Passenger { get; set; }
    public DriverDto? Driver { get; set; }
    public VehicleDto? Vehicle { get; set; }
    public TripRatingDto? Rating { get; set; }
    public TripPaymentDto? Payment { get; set; }
}

public class RequestTripDto
{
    // Origin Information
    public string OriginAddress { get; set; } = string.Empty;
    public double OriginLatitude { get; set; }
    public double OriginLongitude { get; set; }
    
    // Destination Information
    public string DestinationAddress { get; set; } = string.Empty;
    public double DestinationLatitude { get; set; }
    public double DestinationLongitude { get; set; }
    
    public VehicleType RequestedVehicleType { get; set; }
    public string? PassengerNotes { get; set; }
}

public class UpdateTripStatusDto
{
    public TripStatus Status { get; set; }
    public string? CancellationReason { get; set; }
} 