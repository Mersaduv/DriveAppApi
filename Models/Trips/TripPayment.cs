using DriveApp.Models.Core;

namespace DriveApp.Models.Trips;

public class TripPayment : BaseEntity
{
    public Guid TripId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, Wallet
    public string? PaymentReference { get; set; }
    public bool IsPaid { get; set; } = false;
    public DateTime? PaidAt { get; set; }
    
    // Navigation Properties
    public virtual Trip Trip { get; set; } = null!;
} 