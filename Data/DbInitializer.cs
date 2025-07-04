using Microsoft.EntityFrameworkCore;
using DriveApp.Models.Users;
using DriveApp.Models.System;
using DriveApp.Enums;

namespace DriveApp.Data;

public class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = loggerFactory.CreateLogger<DbInitializer>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            logger.LogInformation("Ensuring database is created...");
            
            // Apply migrations if they are pending
            await context.Database.MigrateAsync();
            
            // Seed the database if it's empty
            await SeedDataAsync(context, logger);
            
            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
    
    private static async Task SeedDataAsync(AppDbContext context, ILogger logger)
    {
        // Add permissions if they don't exist
        if (!await context.Permissions.AnyAsync())
        {
            logger.LogInformation("Seeding permissions...");
            
            var permissions = Enum.GetValues(typeof(PermissionType))
                .Cast<PermissionType>()
                .Select(p => new Permission
                {
                    Name = p.ToString(),
                    PermissionType = p,
                    Description = GetPermissionDescription(p),
                    Category = GetPermissionCategory(p)
                })
                .ToList();
                
            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Successfully seeded {Count} permissions", permissions.Count);
        }
        
        // Add roles if they don't exist
        if (!await context.Roles.AnyAsync())
        {
            logger.LogInformation("Seeding roles...");
            
            // Super Admin role
            var superAdminRole = new Role
            {
                Name = "SuperAdmin",
                Description = "Has complete access to all functionalities",
                RoleType = UserRoleType.SuperAdmin,
                IsSystemRole = true
            };
            await context.Roles.AddAsync(superAdminRole);
            await context.SaveChangesAsync();
            
            // Get all permissions
            var allPermissions = await context.Permissions.ToListAsync();
            
            // Assign all permissions to SuperAdmin
            foreach (var permission in allPermissions)
            {
                await context.RolePermissions.AddAsync(new RolePermission
                {
                    RoleId = superAdminRole.Id,
                    PermissionId = permission.Id
                });
            }
            
            // Admin role
            var adminRole = new Role
            {
                Name = "Admin",
                Description = "Has access to administrative functionalities",
                RoleType = UserRoleType.Admin,
                IsSystemRole = true
            };
            await context.Roles.AddAsync(adminRole);
            
            // Driver Registrar role
            var driverRegistrarRole = new Role
            {
                Name = "DriverRegistrar",
                Description = "Can manage driver registrations and approvals",
                RoleType = UserRoleType.DriverRegistrar,
                IsSystemRole = true
            };
            await context.Roles.AddAsync(driverRegistrarRole);
            
            // Driver role
            var driverRole = new Role
            {
                Name = "Driver",
                Description = "Can accept and complete rides",
                RoleType = UserRoleType.Driver,
                IsSystemRole = true
            };
            await context.Roles.AddAsync(driverRole);
            
            // Passenger role
            var passengerRole = new Role
            {
                Name = "Passenger",
                Description = "Can request rides",
                RoleType = UserRoleType.Passenger,
                IsSystemRole = true
            };
            await context.Roles.AddAsync(passengerRole);
            
            await context.SaveChangesAsync();
            
            logger.LogInformation("Successfully seeded roles");
        }
        
        // Add default price configurations if they don't exist
        if (!await context.PriceConfigurations.AnyAsync())
        {
            logger.LogInformation("Seeding price configurations...");
            
            var priceConfigurations = new List<PriceConfiguration>
            {
                new PriceConfiguration
                {
                    VehicleType = VehicleType.NormalCar,
                    BasePrice = 5000, // In local currency units
                    PricePerKm = 1000,
                    PricePerMinute = 100,
                    MinimumPrice = 10000,
                    IsActive = true,
                    Description = "Standard car pricing"
                },
                new PriceConfiguration
                {
                    VehicleType = VehicleType.LuxuryVehicle,
                    BasePrice = 10000,
                    PricePerKm = 2000,
                    PricePerMinute = 200,
                    MinimumPrice = 20000,
                    IsActive = true,
                    Description = "Luxury vehicle pricing"
                },
                new PriceConfiguration
                {
                    VehicleType = VehicleType.Motorcycle,
                    BasePrice = 3000,
                    PricePerKm = 700,
                    PricePerMinute = 70,
                    MinimumPrice = 7000,
                    IsActive = true,
                    Description = "Motorcycle pricing"
                },
                new PriceConfiguration
                {
                    VehicleType = VehicleType.Rickshaw,
                    BasePrice = 2500,
                    PricePerKm = 600,
                    PricePerMinute = 60,
                    MinimumPrice = 6000,
                    IsActive = true,
                    Description = "Rickshaw pricing"
                },
                new PriceConfiguration
                {
                    VehicleType = VehicleType.Taxi,
                    BasePrice = 4000,
                    PricePerKm = 900,
                    PricePerMinute = 90,
                    MinimumPrice = 8000,
                    IsActive = true,
                    Description = "Taxi pricing"
                },
                new PriceConfiguration
                {
                    VehicleType = VehicleType.Van,
                    BasePrice = 7000,
                    PricePerKm = 1500,
                    PricePerMinute = 150,
                    MinimumPrice = 15000,
                    IsActive = true,
                    Description = "Van pricing"
                }
            };
            
            await context.PriceConfigurations.AddRangeAsync(priceConfigurations);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Successfully seeded price configurations");
        }
        
        // Add default system settings if they don't exist
        if (!await context.SystemSettings.AnyAsync())
        {
            logger.LogInformation("Seeding system settings...");
            
            var systemSettings = new List<SystemSetting>
            {
                new SystemSetting
                {
                    Key = "AppName",
                    Value = "RideApp",
                    Description = "Name of the application",
                    Category = "General",
                    IsPublic = true
                },
                new SystemSetting
                {
                    Key = "DefaultLanguage",
                    Value = "en-US",
                    Description = "Default language for the application",
                    Category = "Localization",
                    IsPublic = true
                },
                new SystemSetting
                {
                    Key = "DefaultCurrency",
                    Value = "IRR",
                    Description = "Default currency for pricing",
                    Category = "Pricing",
                    IsPublic = true
                },
                new SystemSetting
                {
                    Key = "MaxDriverSearchRadius",
                    Value = "5", // In kilometers
                    Description = "Maximum radius to search for available drivers",
                    Category = "Trips",
                    IsPublic = false
                },
                new SystemSetting
                {
                    Key = "DefaultTripTimeout",
                    Value = "300", // In seconds (5 minutes)
                    Description = "Default timeout for trip requests",
                    Category = "Trips",
                    IsPublic = false
                },
                new SystemSetting
                {
                    Key = "DriverArrivalTimeout",
                    Value = "600", // In seconds (10 minutes)
                    Description = "Maximum time a driver has to arrive after accepting a trip",
                    Category = "Trips",
                    IsPublic = false
                },
                new SystemSetting
                {
                    Key = "EnableDriverRegistration",
                    Value = "true",
                    Description = "Whether driver registration is enabled",
                    Category = "Registration",
                    IsPublic = true
                }
            };
            
            await context.SystemSettings.AddRangeAsync(systemSettings);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Successfully seeded system settings");
        }
    }
    
    private static string GetPermissionDescription(PermissionType permissionType)
    {
        return permissionType switch
        {
            // User Management
            PermissionType.ViewUsers => "Can view user information",
            PermissionType.CreateUser => "Can create new users",
            PermissionType.EditUser => "Can edit user information",
            PermissionType.DeleteUser => "Can delete users",
            
            // Driver Management
            PermissionType.ViewDrivers => "Can view driver information",
            PermissionType.ApproveDriver => "Can approve driver registrations",
            PermissionType.RejectDriver => "Can reject driver registrations",
            PermissionType.SuspendDriver => "Can suspend drivers",
            
            // Trip Management
            PermissionType.ViewTrips => "Can view trip information",
            PermissionType.ManageTrips => "Can manage trips",
            PermissionType.ViewTripReports => "Can view trip reports",
            
            // System Settings
            PermissionType.ManageRoles => "Can manage roles and permissions",
            PermissionType.ManagePermissions => "Can manage permissions",
            PermissionType.SystemSettings => "Can manage system settings",
            
            _ => "No description available"
        };
    }
    
    private static string GetPermissionCategory(PermissionType permissionType)
    {
        return permissionType switch
        {
            PermissionType.ViewUsers or
            PermissionType.CreateUser or
            PermissionType.EditUser or
            PermissionType.DeleteUser => "User Management",
            
            PermissionType.ViewDrivers or
            PermissionType.ApproveDriver or
            PermissionType.RejectDriver or
            PermissionType.SuspendDriver => "Driver Management",
            
            PermissionType.ViewTrips or
            PermissionType.ManageTrips or
            PermissionType.ViewTripReports => "Trip Management",
            
            PermissionType.ManageRoles or
            PermissionType.ManagePermissions or
            PermissionType.SystemSettings => "System Settings",
            
            _ => "Other"
        };
    }
} 