namespace DriveApp.DTOs.Trips;

public class TripRatingDto
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public int PassengerRating { get; set; } // 1-5
    public int? DriverRating { get; set; } // 1-5
    public string? PassengerComment { get; set; }
    public string? DriverComment { get; set; }
    public DateTime? RatedAt { get; set; }
}

public class SubmitPassengerRatingDto
{
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}

public class SubmitDriverRatingDto
{
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
} 