using DriveApp.DTOs.Users;

namespace DriveApp.DTOs.Passengers;

public class PassengerDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Rating { get; set; }
    public int TotalTrips { get; set; }
    public string? PreferredPaymentMethod { get; set; }
    public UserDto? User { get; set; }
    public List<PassengerFavoriteLocationDto>? FavoriteLocations { get; set; }
}

public class PassengerRegistrationDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PreferredPaymentMethod { get; set; }
}

public class UpdatePassengerDto
{
    public string? PreferredPaymentMethod { get; set; }
} 