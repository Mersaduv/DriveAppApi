using DriveApp.Models.Core;
using DriveApp.Models.Trips;
using DriveApp.Models.Users;

namespace DriveApp.Models.Passengers;

public class Passenger : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Rating { get; set; } = 0;
    public int TotalTrips { get; set; } = 0;
    public string? PreferredPaymentMethod { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public virtual ICollection<PassengerFavoriteLocation> FavoriteLocations { get; set; } = new List<PassengerFavoriteLocation>();
} 