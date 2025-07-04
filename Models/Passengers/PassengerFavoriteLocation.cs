using DriveApp.Models.Core;

namespace DriveApp.Models.Passengers;

public class PassengerFavoriteLocation : BaseEntity
{
    public Guid PassengerId { get; set; }
    public string Name { get; set; } = string.Empty; // خانه، محل کار و...
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    // Navigation Properties
    public virtual Passenger Passenger { get; set; } = null!;
} 