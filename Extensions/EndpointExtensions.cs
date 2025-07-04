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
    public static void MapApiEndpoints(this WebApplication app)
    {
        // Map authentication endpoints
        app.MapAuthEndpoints();
        
        // Map user management endpoints
        app.MapUserEndpoints();
        app.MapRoleEndpoints();
        
        // Map driver related endpoints
        app.MapDriverEndpoints();
        app.MapDocumentEndpoints();
        app.MapVehicleEndpoints();
        
        // Map passenger related endpoints
        app.MapPassengerEndpoints();
        
        // Map trip related endpoints
        app.MapTripEndpoints();
        
        // Map system related endpoints
        app.MapSystemSettingsEndpoints();
        app.MapPriceConfigurationEndpoints();
        
        // Map WebSocket endpoints
        app.MapWebSocketEndpoints();
    }
} 