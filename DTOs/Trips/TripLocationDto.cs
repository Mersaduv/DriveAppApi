namespace DriveApp.DTOs.Trips;

public class TripLocationDto
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
    public double? Speed { get; set; }
    public double? Heading { get; set; }
} 