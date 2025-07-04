using DriveApp.Models.Core;
using DriveApp.Models.Users;
using DriveApp.Models.Trips;
using DriveApp.Enums;

namespace DriveApp.Models.Drivers;

public class Driver : BaseEntity
{
    public Guid UserId { get; set; }
    public string? NationalCardNumber { get; set; }
    public string FullAddress { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DriverStatus Status { get; set; } = DriverStatus.Pending;
    public decimal Rating { get; set; } = 0;
    public int TotalTrips { get; set; } = 0;
    public bool IsOnline { get; set; } = false;
    public DateTime? LastLocationUpdate { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public virtual ICollection<DriverLocation> LocationHistory { get; set; } = new List<DriverLocation>();
} 