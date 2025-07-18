using DriveApp.Enums;
using DriveApp.DTOs.Users;

namespace DriveApp.DTOs.Drivers;

public class DriverDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? NationalCardNumber { get; set; }
    public string FullAddress { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DriverStatus Status { get; set; }
    public decimal Rating { get; set; }
    public int TotalTrips { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastLocationUpdate { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public UserDto? User { get; set; }
    public List<VehicleDto>? Vehicles { get; set; }
}

public class DriverRegistrationDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string NationalCardNumber { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
}

public class UpdateDriverStatusDto
{
    public DriverStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}

public class DriverLocationUpdateDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Speed { get; set; }
    public double? Heading { get; set; }
} 