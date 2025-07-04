using DriveApp.Models.Core;
using DriveApp.Models.Trips;
using DriveApp.Enums;

namespace DriveApp.Models.Drivers;

public class Vehicle : BaseEntity
{
    public Guid DriverId { get; set; }
    public VehicleType VehicleType { get; set; }
    public string Make { get; set; } = string.Empty;         // نام خودرو (پژو، پراید و...)
    public string Model { get; set; } = string.Empty;        // مدل خودرو
    public string? Year { get; set; }
    public string? Color { get; set; }
    public string? PlateNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } = false;
    
    // Navigation Properties
    public virtual Driver Driver { get; set; } = null!;
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
} 