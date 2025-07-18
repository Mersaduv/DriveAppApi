using Microsoft.EntityFrameworkCore;
using DriveApp.Data;
using DriveApp.Models.Users;
using DriveApp.DTOs.Users;
using DriveApp.Services.Helpers;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DriveApp.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext dbContext,
        ILogger<AuthService> logger,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
    }

    // Generate a random 6-digit verification code
    public string GenerateVerificationCode()
    {
        Random random = new Random();
        return random.Next(100000, 1000000).ToString();
    }

    // Request phone verification and save code to database
    public async Task<string> RequestPhoneVerificationAsync(string phoneNumber, string userType = "passenger")
    {
        _logger.LogInformation("RequestPhoneVerificationAsync called with phone: {PhoneNumber}, userType: {UserType}", phoneNumber, userType);
        
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogWarning("Empty phone number provided");
            throw new ArgumentException("Phone number cannot be empty");
        }
        
        // Validate userType
        if (userType != "passenger" && userType != "driver")
        {
            _logger.LogWarning("Invalid user type: {UserType}", userType);
            userType = "passenger"; // Default to passenger
        }

        // Normalize the phone number: remove spaces, dashes, parentheses
        phoneNumber = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
        
        // Add Afghanistan country code (+93) if not present
        if (!phoneNumber.StartsWith("+"))
        {
            // Remove any leading zeros
            phoneNumber = phoneNumber.TrimStart('0');
            // Add the country code
            phoneNumber = "+93" + phoneNumber;
            _logger.LogInformation("Added country code, phone number now: {PhoneNumber}", phoneNumber);
        }

        // Validate Afghan phone numbers
        // Expected format: +93 followed by 9 or 10 digits
        if (!phoneNumber.StartsWith("+93") || 
            (phoneNumber.Length != 12 && phoneNumber.Length != 13))
        {
            _logger.LogWarning("Invalid Afghan phone number format: {PhoneNumber}", phoneNumber);
        }

        // Generate a 6-digit code
        var verificationCode = GenerateVerificationCode();
        _logger.LogInformation("Generated verification code {Code} for {PhoneNumber}", verificationCode, phoneNumber);

        try
        {
            // Check if there's an existing verification for this phone number
            var existingVerification = await _dbContext.PhoneVerifications
                .Where(v => v.PhoneNumber == phoneNumber && !v.IsUsed && v.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            // Check if user exists (but don't throw any errors if they do)
            var userExists = await _dbContext.Users
                .AnyAsync(u => u.PhoneNumber == phoneNumber);
            
            if (userExists)
            {
                _logger.LogInformation("User already exists with phone number: {PhoneNumber}, proceeding with verification", phoneNumber);
            }

            if (existingVerification != null)
            {
                _logger.LogInformation("Updating existing verification for {PhoneNumber}", phoneNumber);
                
                // Update the existing verification
                existingVerification.VerificationCode = verificationCode;
                existingVerification.ExpiresAt = DateTime.UtcNow.AddMinutes(10); // Expire in 10 minutes
                existingVerification.UserType = userType;
                existingVerification.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _logger.LogInformation("Creating new verification for {PhoneNumber}", phoneNumber);
                
                // Find user by phone number if exists
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

                // Create a new verification
                var verification = new PhoneVerification
                {
                    PhoneNumber = phoneNumber,
                    VerificationCode = verificationCode,
                    UserType = userType,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10), // Expire in 10 minutes
                    UserId = user?.Id // Set UserId if user exists, otherwise it will be null
                };

                await _dbContext.PhoneVerifications.AddAsync(verification);
            }

            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Successfully saved verification code for {PhoneNumber}", phoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing verification for {PhoneNumber}", phoneNumber);
            throw;
        }

        return verificationCode; // In a real app, you'd return a requestId and send the code via SMS
    }

    // Verify the phone number with the code
    public async Task<(bool Success, string? Message, UserDto? User, string? Token, string UserType)> VerifyPhoneNumberAsync(string phoneNumber, string verificationCode)
    {
        _logger.LogInformation("VerifyPhoneNumberAsync called with phone: {PhoneNumber}, code: {Code}", phoneNumber, verificationCode);
        
        if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(verificationCode))
        {
            _logger.LogWarning("Phone number or verification code is empty");
            return (false, "Phone number and verification code are required", null, null, "");
        }

        // Normalize the phone number: remove spaces, dashes, parentheses
        phoneNumber = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
        
        // Add Afghanistan country code (+93) if not present
        if (!phoneNumber.StartsWith("+"))
        {
            // Remove any leading zeros
            phoneNumber = phoneNumber.TrimStart('0');
            // Add the country code
            phoneNumber = "+93" + phoneNumber;
            _logger.LogInformation("Added country code, phone number now: {PhoneNumber}", phoneNumber);
        }

        try
        {
            // Find the verification record
            var verification = await _dbContext.PhoneVerifications
                .Where(v => v.PhoneNumber == phoneNumber && v.VerificationCode == verificationCode && !v.IsUsed && v.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (verification == null)
            {
                _logger.LogWarning("Invalid or expired verification code for {PhoneNumber}", phoneNumber);
                
                // Check if there's any verification for this number for better error reporting
                var anyVerification = await _dbContext.PhoneVerifications
                    .Where(v => v.PhoneNumber == phoneNumber)
                    .OrderByDescending(v => v.CreatedAt)
                    .FirstOrDefaultAsync();
                
                if (anyVerification == null)
                {
                    return (false, "No verification code has been requested for this number", null, null, "");
                }
                else if (anyVerification.IsUsed)
                {
                    return (false, "Verification code has already been used", null, null, "");
                }
                else if (anyVerification.ExpiresAt <= DateTime.UtcNow)
                {
                    return (false, "Verification code has expired", null, null, "");
                }
                else
                {
                    return (false, "Invalid verification code", null, null, "");
                }
            }

            string userType = verification.UserType;

            // Mark the verification as used
            verification.IsUsed = true;
            verification.VerifiedAt = DateTime.UtcNow;
            
            _logger.LogInformation("Valid verification code for {PhoneNumber}", phoneNumber);

            // Find or create the user
            var user = await _dbContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            bool isNewUser = false;
            
            if (user == null)
            {
                _logger.LogInformation("Creating new user for {PhoneNumber}", phoneNumber);
                
                // Create a new user
                user = new User
                {
                    PhoneNumber = phoneNumber,
                    IsPhoneVerified = true,
                    PhoneVerifiedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                await _dbContext.Users.AddAsync(user);
                isNewUser = true;
            }
            else
            {
                _logger.LogInformation("Updating existing user for {PhoneNumber}", phoneNumber);
                
                // Update existing user
                user.IsPhoneVerified = true;
                user.PhoneVerifiedAt = DateTime.UtcNow;
                user.LastLoginAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);
            _logger.LogInformation("Generated JWT token for user {UserId}", user.Id);

            // For existing users, we want to make sure the flow continues smoothly
            // We'll return success with an appropriate message
            var message = isNewUser 
                ? "New user created and verified" 
                : "Phone verified successfully";

            return (true, message, user.ToDto(), token, userType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying phone number {PhoneNumber}", phoneNumber);
            return (false, $"Error: {ex.Message}", null, null, "");
        }
    }

    // Generate JWT token
    public string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
        };

        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        if (!string.IsNullOrEmpty(user.FullName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.FullName));
        }

        // Add user roles
        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DefaultSecretKeyWithAtLeast32Characters!!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(1);

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
} 