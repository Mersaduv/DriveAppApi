using DriveApp.Enums;

namespace DriveApp.DTOs.System;

public class PriceConfigurationDto
{
    public Guid Id { get; set; }
    public VehicleType VehicleType { get; set; }
    public decimal BasePrice { get; set; }
    public decimal PricePerKm { get; set; }
    public decimal PricePerMinute { get; set; }
    public decimal MinimumPrice { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
}

public class CreatePriceConfigurationDto
{
    public VehicleType VehicleType { get; set; }
    public decimal BasePrice { get; set; }
    public decimal PricePerKm { get; set; }
    public decimal PricePerMinute { get; set; }
    public decimal MinimumPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}

public class UpdatePriceConfigurationDto
{
    public decimal? BasePrice { get; set; }
    public decimal? PricePerKm { get; set; }
    public decimal? PricePerMinute { get; set; }
    public decimal? MinimumPrice { get; set; }
    public bool? IsActive { get; set; }
    public string? Description { get; set; }
} 