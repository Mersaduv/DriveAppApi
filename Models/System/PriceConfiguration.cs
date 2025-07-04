using DriveApp.Models.Core;
using DriveApp.Enums;

namespace DriveApp.Models.System;

public class PriceConfiguration : BaseEntity
{
    public VehicleType VehicleType { get; set; }
    public decimal BasePrice { get; set; }        // Base price
    public decimal PricePerKm { get; set; }       // Price per kilometer
    public decimal PricePerMinute { get; set; }   // Price per minute
    public decimal MinimumPrice { get; set; }     // Minimum price
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
} 