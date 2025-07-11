using DriveApp.DTOs.Drivers;
using DriveApp.Enums;
using DriveApp.Services.Drivers;

namespace DriveApp.Endpoints.Drivers;

public static class DriverEndpoints
{
    public static void MapDriverEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/drivers").WithTags("Drivers");

        // Get all drivers with optional filtering
        group.MapGet("/", async (
            IDriverService driverService, 
            string? status = null, 
            bool? isOnline = null, 
            int page = 1, 
            int pageSize = 10) =>
        {
            DriverStatus? statusEnum = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DriverStatus>(status, true, out var parsedStatus))
            {
                statusEnum = parsedStatus;
            }

            if (statusEnum.HasValue)
            {
                return Results.Ok(await driverService.GetDriversByStatusAsync(statusEnum.Value));
            }
            
            return Results.Ok(await driverService.GetAllDriversAsync());
        })
        .WithName("GetAllDrivers")
        .WithOpenApi();

        // Get a specific driver by ID
        group.MapGet("/{id}", async (Guid id, IDriverService driverService) =>
        {
            var driver = await driverService.GetDriverByIdAsync(id);
            if (driver == null)
                return Results.NotFound();

            return Results.Ok(driver);
        })
        .WithName("GetDriverById")
        .WithOpenApi();

        // Create a new driver
        group.MapPost("/", async (DriverRegistrationDto driverDto, IDriverService driverService) =>
        {
            var result = await driverService.CreateDriverAsync(driverDto);
            return Results.Created($"/api/drivers/{result.Id}", result);
        })
        .WithName("CreateDriver")
        .WithOpenApi();

        // Update a driver status
        group.MapPut("/{id}", async (Guid id, UpdateDriverStatusDto driverDto, IDriverService driverService) =>
        {
            var result = await driverService.UpdateDriverStatusAsync(id, driverDto);
            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("UpdateDriver")
        .WithOpenApi();

        // Delete a driver
        group.MapDelete("/{id}", async (Guid id, IDriverService driverService) =>
        {
            var result = await driverService.DeleteDriverAsync(id);
            if (!result)
                return Results.NotFound();

            return Results.NoContent();
        })
        .WithName("DeleteDriver")
        .WithOpenApi();

        // Update driver status
        group.MapPatch("/{id}/status", async (Guid id, UpdateDriverStatusDto statusDto, IDriverService driverService) =>
        {
            var result = await driverService.UpdateDriverStatusAsync(id, statusDto);
            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("UpdateDriverStatus")
        .WithOpenApi();

        // Update driver online status
        group.MapPatch("/{id}/online-status", async (Guid id, bool isOnline, IDriverService driverService) =>
        {
            var result = await driverService.ToggleDriverOnlineStatusAsync(id, isOnline);
            if (!result)
                return Results.NotFound();

            return Results.Ok();
        })
        .WithName("UpdateDriverOnlineStatus")
        .WithOpenApi();

        // Update driver location
        group.MapPatch("/{id}/location", async (Guid id, DriverLocationUpdateDto locationDto, IDriverService driverService) =>
        {
            var result = await driverService.UpdateDriverLocationAsync(id, locationDto);
            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("UpdateDriverLocation")
        .WithOpenApi();
    }
} 