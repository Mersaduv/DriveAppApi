using DriveApp.DTOs.Passengers;
using DriveApp.Services.Passengers;

namespace DriveApp.Endpoints.Passengers;

public static class PassengerEndpoints
{
    public static void MapPassengerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/passengers").WithTags("Passengers");

        // Get all passengers
        group.MapGet("/", async (IPassengerService passengerService) =>
        {
            var result = await passengerService.GetAllPassengersAsync();
            return Results.Ok(result);
        })
        .WithName("GetAllPassengers")
        .WithOpenApi();

        // Get a specific passenger by ID
        group.MapGet("/{id}", async (Guid id, IPassengerService passengerService) =>
        {
            var passenger = await passengerService.GetPassengerByIdAsync(id);
            if (passenger == null)
                return Results.NotFound();

            return Results.Ok(passenger);
        })
        .WithName("GetPassengerById")
        .WithOpenApi();

        // Get passenger by user ID
        group.MapGet("/user/{userId}", async (Guid userId, IPassengerService passengerService) =>
        {
            var passenger = await passengerService.GetPassengerByUserIdAsync(userId);
            if (passenger == null)
                return Results.NotFound();

            return Results.Ok(passenger);
        })
        .WithName("GetPassengerByUserId")
        .WithOpenApi();

        // Create a new passenger
        group.MapPost("/", async (PassengerRegistrationDto passengerDto, IPassengerService passengerService) =>
        {
            var result = await passengerService.CreatePassengerAsync(passengerDto);
            return Results.Created($"/api/passengers/{result.Id}", result);
        })
        .WithName("CreatePassenger")
        .WithOpenApi();

        // Update a passenger
        group.MapPut("/{id}", async (Guid id, UpdatePassengerDto passengerDto, IPassengerService passengerService) =>
        {
            var result = await passengerService.UpdatePassengerAsync(id, passengerDto);
            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("UpdatePassenger")
        .WithOpenApi();

        // Delete a passenger
        group.MapDelete("/{id}", async (Guid id, IPassengerService passengerService) =>
        {
            var result = await passengerService.DeletePassengerAsync(id);
            if (!result)
                return Results.NotFound();

            return Results.NoContent();
        })
        .WithName("DeletePassenger")
        .WithOpenApi();

        // Get passenger's favorite locations
        group.MapGet("/{id}/favorite-locations", async (Guid id, IPassengerService passengerService) =>
        {
            var favoriteLocations = await passengerService.GetFavoriteLocationsAsync(id);
            return Results.Ok(favoriteLocations);
        })
        .WithName("GetPassengerFavoriteLocations")
        .WithOpenApi();

        // Add favorite location for passenger
        group.MapPost("/{id}/favorite-locations", async (
            Guid id, 
            CreateFavoriteLocationDto favoriteLocationDto, 
            IPassengerService passengerService) =>
        {
            var result = await passengerService.AddFavoriteLocationAsync(id, favoriteLocationDto);
            return Results.Created($"/api/passengers/{id}/favorite-locations/{result.Id}", result);
        })
        .WithName("AddPassengerFavoriteLocation")
        .WithOpenApi();

        // Update favorite location
        group.MapPut("/favorite-locations/{locationId}", async (
            Guid locationId,
            UpdateFavoriteLocationDto favoriteLocationDto,
            IPassengerService passengerService) =>
        {
            var result = await passengerService.UpdateFavoriteLocationAsync(locationId, favoriteLocationDto);
                
            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("UpdatePassengerFavoriteLocation")
        .WithOpenApi();

        // Delete favorite location
        group.MapDelete("/favorite-locations/{locationId}", async (
            Guid locationId,
            IPassengerService passengerService) =>
        {
            var result = await passengerService.DeleteFavoriteLocationAsync(locationId);
            if (!result)
                return Results.NotFound();

            return Results.NoContent();
        })
        .WithName("DeletePassengerFavoriteLocation")
        .WithOpenApi();
    }
} 