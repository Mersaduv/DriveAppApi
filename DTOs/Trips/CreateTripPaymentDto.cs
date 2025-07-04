namespace DriveApp.DTOs.Trips;

public class CreateTripPaymentDto
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, Wallet
    public string? PaymentReference { get; set; }
} 