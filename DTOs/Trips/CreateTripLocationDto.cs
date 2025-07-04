namespace DriveApp.DTOs.Trips;

public class CreateTripLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Speed { get; set; }
    public double? Heading { get; set; }
} 