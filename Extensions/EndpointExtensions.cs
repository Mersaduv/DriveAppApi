using DriveApp.Endpoints.Auth;
using DriveApp.Endpoints.Drivers;
using DriveApp.Endpoints.Passengers;
using DriveApp.Endpoints.System;
using DriveApp.Endpoints.Trips;
using DriveApp.Endpoints.Users;
using DriveApp.Endpoints.WebSocket;

namespace DriveApp.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapAllApiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Auth endpoints
        endpoints.MapAuthEndpoints();
        
        // User management endpoints
        endpoints.MapUserEndpoints();
        endpoints.MapRoleEndpoints();
        
        // Driver management endpoints
        endpoints.MapDriverEndpoints();
        endpoints.MapVehicleEndpoints();
        endpoints.MapDocumentEndpoints();
        
        // Passenger management endpoints
        endpoints.MapPassengerEndpoints();
        
        // Trip management endpoints
        endpoints.MapTripEndpoints();
        
        // System management endpoints
        endpoints.MapSystemSettingsEndpoints();
        endpoints.MapPriceConfigurationEndpoints();
        
        // WebSocket endpoints
        endpoints.MapWebSocketEndpoints();
        
        return endpoints;
    }
    
    public static IEndpointRouteBuilder MapApiStatusEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/status", () => new { status = "OK", version = "1.0", timestamp = DateTime.UtcNow })
            .WithName("GetApiStatus")
            .WithTags("Status");
            
        return endpoints;
    }
    
    public static WebApplication UseWebSocketConfiguration(this WebApplication app)
    {
        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(30)
        };
        
        app.UseWebSockets(webSocketOptions);
        
        return app;
    }
    
    public static WebApplication UseSwaggerConfiguration(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "DriveApp API v1");
            options.RoutePrefix = string.Empty;
        });
        
        return app;
    }
    
    public static WebApplication UseCorsConfiguration(this WebApplication app)
    {
        app.UseCors("AllowAll");
        
        return app;
    }
} 