using DriveApp.DTOs.Users;
using DriveApp.Models.Users;

namespace DriveApp.Services;

public interface IAuthService
{
    // Generate a random 6-digit verification code
    string GenerateVerificationCode();
    
    // Request phone verification and save code to database
    Task<string> RequestPhoneVerificationAsync(string phoneNumber, string userType = "passenger");
    
    // Verify the phone number with the code
    Task<(bool Success, string? Message, UserDto? User, string? Token, string UserType)> VerifyPhoneNumberAsync(string phoneNumber, string verificationCode);
    
    // Generate JWT token
    string GenerateJwtToken(User user);
} 