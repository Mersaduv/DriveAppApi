using DriveApp.Models.Core;

namespace DriveApp.Models.Drivers;

public class DriverLocation : BaseEntity
{
    public Guid DriverId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double? Speed { get; set; }
    public double? Heading { get; set; }
    
    // Navigation Properties
    public virtual Driver Driver { get; set; } = null!;
} 