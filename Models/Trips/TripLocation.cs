using DriveApp.Models.Core;

namespace DriveApp.Models.Trips;

public class TripLocation : BaseEntity
{
    public Guid TripId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double? Speed { get; set; }
    public double? Heading { get; set; }
    
    // Navigation Properties
    public virtual Trip Trip { get; set; } = null!;
} 