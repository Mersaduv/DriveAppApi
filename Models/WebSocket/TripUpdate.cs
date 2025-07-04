using DriveApp.Enums;

namespace DriveApp.Models.WebSocket;

public class TripUpdate
{
    public Guid TripId { get; set; }
    public TripStatus Status { get; set; }
    public string? Message { get; set; }
    public object? AdditionalData { get; set; }
} 