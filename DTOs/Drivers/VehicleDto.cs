using DriveApp.Enums;

namespace DriveApp.DTOs.Drivers;

public class VehicleDto
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public VehicleType VehicleType { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Year { get; set; }
    public string? Color { get; set; }
    public string? PlateNumber { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public List<DocumentDto>? Documents { get; set; }
}

public class CreateVehicleDto
{
    public VehicleType VehicleType { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Year { get; set; }
    public string? Color { get; set; }
    public string? PlateNumber { get; set; }
}

public class UpdateVehicleDto
{
    public VehicleType? VehicleType { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? Year { get; set; }
    public string? Color { get; set; }
    public string? PlateNumber { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsVerified { get; set; }
} 