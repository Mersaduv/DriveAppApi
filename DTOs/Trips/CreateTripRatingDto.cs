namespace DriveApp.DTOs.Trips;

public class CreateTripRatingDto
{
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public bool IsFromPassenger { get; set; } // True if passenger is rating driver, False if driver is rating passenger
} 