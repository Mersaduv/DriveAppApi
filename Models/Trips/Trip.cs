using DriveApp.Models.Core;
using DriveApp.Models.Drivers;
using DriveApp.Models.Passengers;
using DriveApp.Enums;

namespace DriveApp.Models.Trips;

public class Trip : BaseEntity
{
    public Guid PassengerId { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? VehicleId { get; set; }
    public string TripCode { get; set; } = string.Empty; // کد یکتا سفر
    
    // Origin Information
    public string OriginAddress { get; set; } = string.Empty;
    public double OriginLatitude { get; set; }
    public double OriginLongitude { get; set; }
    
    // Destination Information
    public string DestinationAddress { get; set; } = string.Empty;
    public double DestinationLatitude { get; set; }
    public double DestinationLongitude { get; set; }
    
    // Trip Details
    public TripStatus Status { get; set; } = TripStatus.Requested;
    public VehicleType RequestedVehicleType { get; set; }
    public decimal EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal? Distance { get; set; } // به کیلومتر
    public int? EstimatedDuration { get; set; } // به دقیقه
    public int? ActualDuration { get; set; } // به دقیقه
    
    // Timing
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DriverArrivedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    // Additional Info
    public string? PassengerNotes { get; set; }
    public string? CancellationReason { get; set; }
    public string? CancelledBy { get; set; } // Driver or Passenger
    
    // Navigation Properties
    public virtual Passenger Passenger { get; set; } = null!;
    public virtual Driver? Driver { get; set; }
    public virtual Vehicle? Vehicle { get; set; }
    public virtual ICollection<TripLocation> TripLocations { get; set; } = new List<TripLocation>();
    public virtual TripRating? Rating { get; set; }
    public virtual TripPayment? Payment { get; set; }
} 