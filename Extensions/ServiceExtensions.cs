using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using DriveApp.Data;
using DriveApp.Services;
using DriveApp.Services.Users;
using DriveApp.Services.Drivers;
using DriveApp.Services.Passengers;
using DriveApp.Services.Trips;
using DriveApp.Services.System;

namespace DriveApp.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RideApp API",
                Version = "v1",
                Description = "Ride Sharing Application API with Minimal API approach"
            });
        });
    }

    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }
    
    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
    }
    
    public static void ConfigureServices(this IServiceCollection services)
    {
        // WebSocket services
        services.AddSignalR();
        services.AddSingleton<IWebSocketService, WebSocketService>();
        
        // Register all application services
        // Register RoleService first
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IDocumentService, DocumentService>();
        
        services.AddScoped<IPassengerService, PassengerService>();
        
        services.AddScoped<ITripService, TripService>();
        
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<IPriceConfigurationService, PriceConfigurationService>();
    }
} 