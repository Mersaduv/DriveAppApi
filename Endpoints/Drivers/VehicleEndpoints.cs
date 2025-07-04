using DriveApp.DTOs.Drivers;
using DriveApp.Services.Drivers;

namespace DriveApp.Endpoints.Drivers;

public static class VehicleEndpoints
{
    public static void MapVehicleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/vehicles").WithTags("Vehicles");

        // Get all vehicles
        group.MapGet("/", async (IVehicleService vehicleService) =>
        {
            var vehicles = await vehicleService.GetAllVehiclesAsync();
            return Results.Ok(vehicles);
        })
        .WithName("GetAllVehicles")
        .WithOpenApi();

        // Get vehicle by ID
        group.MapGet("/{id}", async (Guid id, IVehicleService vehicleService) =>
        {
            var vehicle = await vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null)
                return Results.NotFound();

            return Results.Ok(vehicle);
        })
        .WithName("GetVehicleById")
        .WithOpenApi();

        // Get vehicles by driver ID
        group.MapGet("/driver/{driverId}", async (Guid driverId, IVehicleService vehicleService) =>
        {
            var vehicles = await vehicleService.GetVehiclesByDriverIdAsync(driverId);
            return Results.Ok(vehicles);
        })
        .WithName("GetVehiclesByDriverId")
        .WithOpenApi();

        // Create a new vehicle
        group.MapPost("/", async (Guid driverId, CreateVehicleDto vehicleDto, IVehicleService vehicleService) =>
        {
            var result = await vehicleService.CreateVehicleAsync(driverId, vehicleDto);
            return Results.Created($"/api/vehicles/{result.Id}", result);
        })
        .WithName("CreateVehicle")
        .WithOpenApi();

        // Update vehicle
        group.MapPut("/{id}", async (Guid id, UpdateVehicleDto vehicleDto, IVehicleService vehicleService) =>
        {
            var result = await vehicleService.UpdateVehicleAsync(id, vehicleDto);
            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("UpdateVehicle")
        .WithOpenApi();

        // Delete vehicle
        group.MapDelete("/{id}", async (Guid id, IVehicleService vehicleService) =>
        {
            var result = await vehicleService.DeleteVehicleAsync(id);
            if (!result)
                return Results.NotFound();

            return Results.NoContent();
        })
        .WithName("DeleteVehicle")
        .WithOpenApi();

        // Verify vehicle
        group.MapPatch("/{id}/verify", async (Guid id, IVehicleService vehicleService) =>
        {
            var result = await vehicleService.VerifyVehicleAsync(id);
            if (!result)
                return Results.NotFound();

            return Results.Ok();
        })
        .WithName("VerifyVehicle")
        .WithOpenApi();
    }
} 