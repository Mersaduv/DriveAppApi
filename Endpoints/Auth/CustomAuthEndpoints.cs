using DriveApp.Services;
using DriveApp.Services.Users;
using Microsoft.AspNetCore.Mvc;
using DriveApp.Services.Passengers;
using DriveApp.DTOs.Passengers;

namespace DriveApp.Endpoints.Auth;

public static class CustomAuthEndpoints
{
    public static void MapCustomAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth/custom")
            .WithTags("Custom Authentication");

        // Request phone verification
        group.MapPost("/request-code", async (
            [FromBody] RequestVerificationDto request,
            IAuthService authService,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Request verification code for phone: {PhoneNumber}", request.PhoneNumber);
            
            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                logger.LogWarning("Empty phone number in request");
                return Results.BadRequest(new { message = "Phone number is required", success = false });
            }

            try
            {
                // Normalize the phone number
                string phoneNumber = request.PhoneNumber.Trim();
                string userType = request.UserType?.ToLower() ?? "passenger";
                
                // Generate and store the verification code
                var code = await authService.RequestPhoneVerificationAsync(phoneNumber, userType);
                
                // This code should be displayed as a notification in the Tapro app
                // In a real production system, we would send the code via SMS
                // For this MVP, we're just returning it in the response
                logger.LogInformation("Successfully generated code for {PhoneNumber} as {UserType}", phoneNumber, userType);
                
                return Results.Ok(new { 
                    message = "Verification code sent to Tapro app", 
                    code = code,
                    verification_code = code, // Extra field for Tapro app to identify the code
                    notification_text = $"Your verification code is: {code}",
                    success = true,
                    phoneNumber = phoneNumber,
                    userType = userType
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error requesting verification code for {PhoneNumber}", request.PhoneNumber);
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Failed to generate verification code"
                );
            }
        })
        .WithName("RequestCode")
        .WithOpenApi();

        // Verify phone number with code
        group.MapPost("/verify-code", async (
            [FromBody] VerifyCodeDto request,
            IAuthService authService,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Verify code request for phone: {PhoneNumber}", request.PhoneNumber);
            
            if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Code))
            {
                logger.LogWarning("Missing phone number or verification code");
                return Results.BadRequest(new { 
                    message = "Phone number and verification code are required",
                    success = false
                });
            }

            try
            {
                // Normalize the phone number
                string phoneNumber = request.PhoneNumber.Trim();
                string code = request.Code.Trim();
                
                // Verify the code
                var result = await authService.VerifyPhoneNumberAsync(phoneNumber, code);

                if (!result.Success)
                {
                    logger.LogWarning("Verification failed: {Message}", result.Message);
                    return Results.BadRequest(new { 
                        message = result.Message,
                        success = false
                    });
                }

                logger.LogInformation("Verification successful for {PhoneNumber}", phoneNumber);
                return Results.Ok(new
                {
                    message = result.Message,
                    token = result.Token,
                    userId = result.User?.Id,
                    isNewUser = result.Message?.Contains("New user created") ?? false,
                    success = true,
                    user = result.User,
                    userType = result.UserType,
                    requiresRegistration = true // Indicates that user needs to complete registration by providing name
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error verifying code for {PhoneNumber}", request.PhoneNumber);
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Failed to verify code"
                );
            }
        })
        .WithName("VerifyCode")
        .WithOpenApi();
        
        // Complete registration after verification - register as passenger
        group.MapPost("/complete-registration", async (
            [FromBody] CompleteRegistrationDto request,
            IPassengerService passengerService,
            IAuthService authService,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Complete registration request for userId: {UserId}", request.UserId);
            
            if (string.IsNullOrEmpty(request.FirstName) || request.UserId == Guid.Empty)
            {
                logger.LogWarning("Missing required fields for registration completion");
                return Results.BadRequest(new { 
                    message = "First name and user ID are required",
                    success = false
                });
            }

            try
            {
                // Register user as a passenger
                var passengerDto = new PassengerRegistrationDto
                {
                    PhoneNumber = request.PhoneNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email
                };
                
                var passenger = await passengerService.CreatePassengerAsync(passengerDto);
                
                logger.LogInformation("Passenger registration completed for user {UserId}", request.UserId);
                return Results.Ok(new
                {
                    message = "Registration completed successfully",
                    passengerId = passenger.Id,
                    userId = passenger.UserId,
                    success = true,
                    passenger = passenger
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error completing registration for user {UserId}", request.UserId);
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Failed to complete registration"
                );
            }
        })
        .WithName("CompleteRegistration")
        .WithOpenApi();
        
        // Simple test endpoint to check API connectivity
        group.MapGet("/ping", () => {
            return Results.Ok(new { 
                message = "API is responding", 
                timestamp = DateTime.UtcNow,
                success = true
            });
        })
        .WithName("PingAuth")
        .WithOpenApi();
    }
}

public class RequestVerificationDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string UserType { get; set; } = "passenger"; // Values: "passenger" or "driver"
}

public class VerifyCodeDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class CompleteRegistrationDto
{
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Email { get; set; }
} 