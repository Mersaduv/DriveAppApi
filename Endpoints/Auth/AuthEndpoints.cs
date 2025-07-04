using DriveApp.DTOs.Users;
using DriveApp.Services.Users;

namespace DriveApp.Endpoints.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth").WithTags("Authentication");

        // Request phone verification
        group.MapPost("/request-verification", async (
            string phoneNumber, 
            IUserService userService) =>
        {
            var result = await userService.RequestPhoneVerificationAsync(phoneNumber);
            return Results.Ok(new { message = "Verification code sent successfully", requestId = result });
        })
        .WithName("RequestVerification")
        .WithOpenApi();

        // Verify phone number
        group.MapPost("/verify-phone", async (
            string phoneNumber, 
            string verificationCode, 
            IUserService userService) =>
        {
            var result = await userService.VerifyPhoneNumberAsync(phoneNumber, verificationCode);
            if (!result.Success)
                return Results.BadRequest(new { message = result.Message });

            return Results.Ok(new { 
                message = "Phone number verified successfully", 
                token = result.Token,
                userId = result.UserId,
                isNewUser = result.IsNewUser
            });
        })
        .WithName("VerifyPhone")
        .WithOpenApi();

        // Login with phone
        group.MapPost("/login", async (
            string phoneNumber, 
            string verificationCode, 
            IUserService userService) =>
        {
            var result = await userService.LoginAsync(phoneNumber, verificationCode);
            if (!result.Success)
                return Results.BadRequest(new { message = result.Message });

            return Results.Ok(new { 
                message = "Login successful", 
                token = result.Token,
                userId = result.UserId,
                user = result.User
            });
        })
        .WithName("Login")
        .WithOpenApi();

        // Refresh token
        group.MapPost("/refresh-token", async (
            string refreshToken, 
            IUserService userService) =>
        {
            var result = await userService.RefreshTokenAsync(refreshToken);
            if (!result.Success)
                return Results.BadRequest(new { message = result.Message });

            return Results.Ok(new { 
                token = result.Token,
                refreshToken = result.RefreshToken
            });
        })
        .WithName("RefreshToken")
        .WithOpenApi();

        // Logout
        group.MapPost("/logout", async (
            string token, 
            IUserService userService) =>
        {
            await userService.LogoutAsync(token);
            return Results.Ok(new { message = "Logout successful" });
        })
        .WithName("Logout")
        .WithOpenApi();

        // Get current user
        group.MapGet("/me", async (
            HttpContext context, 
            IUserService userService) =>
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Results.Unauthorized();

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var user = await userService.GetUserFromTokenAsync(token);
            if (user == null)
                return Results.Unauthorized();

            return Results.Ok(user);
        })
        .WithName("GetCurrentUser")
        .WithOpenApi();
    }
} 