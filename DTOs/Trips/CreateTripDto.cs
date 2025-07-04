using DriveApp.Enums;

namespace DriveApp.DTOs.Trips;

public class CreateTripDto
{
    public Guid PassengerId { get; set; }
    
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