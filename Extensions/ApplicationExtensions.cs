using DriveApp.Endpoints.Auth;
using DriveApp.Endpoints.Drivers;
using DriveApp.Endpoints.Passengers;
using DriveApp.Endpoints.System;
using DriveApp.Endpoints.Trips;
using DriveApp.Endpoints.Users;
using DriveApp.Endpoints.WebSocket;

namespace DriveApp.Extensions;

public static class ApplicationExtensions
{
    // No longer needed as it's implemented in EndpointExtensions
    // public static void UseSwaggerConfiguration(this WebApplication app)
    // {
    //     app.UseSwagger();
    //     app.UseSwaggerUI(c =>
    //     {
    //         c.SwaggerEndpoint("/swagger/v1/swagger.json", "RideApp API v1");
    //         c.RoutePrefix = "swagger";
    //     });
    // }

    // No longer needed as it's implemented in EndpointExtensions
    // public static void UseCorsConfiguration(this WebApplication app)
    // {
    //     app.UseCors("CorsPolicy");
    // }

    // No longer needed as it's implemented in EndpointExtensions
    // public static void UseWebSocketConfiguration(this WebApplication app)
    // {
    //     var webSocketOptions = new WebSocketOptions
    //     {
    //         KeepAliveInterval = TimeSpan.FromMinutes(2)
    //     };
    //     
    //     app.UseWebSockets(webSocketOptions);
    // }

    // No longer needed as it's implemented in EndpointExtensions
    // public static void MapApiStatusEndpoint(this WebApplication app)
    // {
    //     app.MapGet("/api/status", () => Results.Ok(new { 
    //             Status = "Running", 
    //             Timestamp = DateTime.UtcNow, 
    //             Message = "DriveApp API is operational" 
    //         }))
    //         .WithName("GetApiStatus")
    //         .WithTags("Status")
    //         .WithOpenApi();
    // }

    // No longer needed as it's implemented in EndpointExtensions
    // public static void MapAllApiEndpoints(this WebApplication app)
    // {
    //     // Authentication endpoints
    //     app.MapAuthEndpoints();
    //     
    //     // User management endpoints
    //     app.MapUserEndpoints();
    //     app.MapRoleEndpoints();
    //     
    //     // Driver related endpoints
    //     app.MapDriverEndpoints();
    //     app.MapDocumentEndpoints();
    //     app.MapVehicleEndpoints();
    //     
    //     // Passenger related endpoints
    //     app.MapPassengerEndpoints();
    //     
    //     // Trip related endpoints
    //     app.MapTripEndpoints();
    //     
    //     // System related endpoints
    //     app.MapSystemSettingsEndpoints();
    //     app.MapPriceConfigurationEndpoints();
    //     
    //     // WebSocket endpoints
    //     app.MapWebSocketEndpoints();
    // }
} 