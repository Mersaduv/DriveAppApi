using DriveApp.Models.Core;

namespace DriveApp.Models.Trips;

public class TripRating : BaseEntity
{
    public Guid TripId { get; set; }
    public int PassengerRating { get; set; } // 1-5
    public int? DriverRating { get; set; } // 1-5
    public string? PassengerComment { get; set; }
    public string? DriverComment { get; set; }
    public DateTime? RatedAt { get; set; }
    
    // Navigation Properties
    public virtual Trip Trip { get; set; } = null!;
} 