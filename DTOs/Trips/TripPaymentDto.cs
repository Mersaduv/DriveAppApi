namespace DriveApp.DTOs.Trips;

public class TripPaymentDto
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, Wallet
    public string? PaymentReference { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class ProcessPaymentDto
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? PaymentReference { get; set; }
    public bool IsPaid { get; set; }
} 