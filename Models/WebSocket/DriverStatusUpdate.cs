namespace DriveApp.Models.WebSocket;

public class DriverStatusUpdate
{
    public Guid DriverId { get; set; }
    public bool IsOnline { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
} 