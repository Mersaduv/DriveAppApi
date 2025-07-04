using DriveApp.DTOs.Trips;
using DriveApp.Enums;
using DriveApp.Services.Trips;

namespace DriveApp.Endpoints.Trips;

public static class TripEndpoints
{
    public static void MapTripEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/trips").WithTags("Trips");

        // Get all trips with optional filtering
        group.MapGet("/", async (
            ITripService tripService, 
            string? status = null, 
            Guid? passengerId = null,
            Guid? driverId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int page = 1, 
            int pageSize = 10) =>
        {
            TripStatus? statusEnum = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TripStatus>(status, true, out var parsedStatus))
            {
                statusEnum = parsedStatus;
            }

            var result = await tripService.GetAllTripsAsync(
                statusEnum, passengerId, driverId, startDate, endDate, page, pageSize);
            return Results.Ok(result);
        })
        .WithName("GetAllTrips")
        .WithOpenApi();

        // Get a specific trip by ID
        group.MapGet("/{id}", async (Guid id, ITripService tripService) =>
        {
            var trip = await tripService.GetTripByIdAsync(id);
            if (trip == null)
                return Results.NotFound();

            return Results.Ok(trip);
        })
        .WithName("GetTripById")
        .WithOpenApi();

        // Get trips by passenger ID
        group.MapGet("/passenger/{passengerId}", async (
            Guid passengerId, 
            ITripService tripService,
            string? status = null,
            int page = 1, 
            int pageSize = 10) =>
        {
            TripStatus? statusEnum = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TripStatus>(status, true, out var parsedStatus))
            {
                statusEnum = parsedStatus;
            }
            
            var trips = await tripService.GetTripsByPassengerIdAsync(passengerId, statusEnum, page, pageSize);
            return Results.Ok(trips);
        })
        .WithName("GetTripsByPassengerId")
        .WithOpenApi();

        // Get trips by driver ID
        group.MapGet("/driver/{driverId}", async (
            Guid driverId, 
            ITripService tripService,
            string? status = null,
            int page = 1, 
            int pageSize = 10) =>
        {
            TripStatus? statusEnum = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TripStatus>(status, true, out var parsedStatus))
            {
                statusEnum = parsedStatus;
            }
            
            var trips = await tripService.GetTripsByDriverIdAsync(driverId, statusEnum, page, pageSize);
            return Results.Ok(trips);
        })
        .WithName("GetTripsByDriverId")
        .WithOpenApi();

        // Create a new trip request
        group.MapPost("/", async (CreateTripDto tripDto, ITripService tripService) =>
        {
            var result = await tripService.CreateTripAsync(tripDto);
            return Results.Created($"/api/trips/{result.Id}", result);
        })
        .WithName("CreateTrip")
        .WithOpenApi();

        // Accept trip by driver
        group.MapPatch("/{id}/accept", async (Guid id, Guid driverId, Guid vehicleId, ITripService tripService) =>
        {
            var result = await tripService.AcceptTripAsync(id, driverId, vehicleId);
            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("AcceptTrip")
        .WithOpenApi();

        // Driver arrived at pickup location
        group.MapPatch("/{id}/driver-arrived", async (Guid id, ITripService tripService) =>
        {
            var result = await tripService.DriverArrivedAsync(id);
            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("DriverArrived")
        .WithOpenApi();

        // Start trip
        group.MapPatch("/{id}/start", async (Guid id, ITripService tripService) =>
        {
            var result = await tripService.StartTripAsync(id);
            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("StartTrip")
        .WithOpenApi();

        // Complete trip
        group.MapPatch("/{id}/complete", async (
            Guid id, 
            double finalLatitude, 
            double finalLongitude,
            ITripService tripService) =>
        {
            var result = await tripService.CompleteTripAsync(id, finalLatitude, finalLongitude);
            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("CompleteTrip")
        .WithOpenApi();

        // Cancel trip
        group.MapPatch("/{id}/cancel", async (
            Guid id, 
            string cancelledBy, 
            string? reason, 
            ITripService tripService) =>
        {
            var result = await tripService.CancelTripAsync(id, cancelledBy, reason);
            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("CancelTrip")
        .WithOpenApi();

        // Add trip location (tracking)
        group.MapPost("/{id}/locations", async (
            Guid id, 
            CreateTripLocationDto locationDto, 
            ITripService tripService) =>
        {
            var result = await tripService.AddTripLocationAsync(id, locationDto);
            if (result == null)
                return Results.NotFound();

            return Results.Created($"/api/trips/{id}/locations/{result.Id}", result);
        })
        .WithName("AddTripLocation")
        .WithOpenApi();

        // Get trip locations
        group.MapGet("/{id}/locations", async (Guid id, ITripService tripService) =>
        {
            var locations = await tripService.GetTripLocationsAsync(id);
            return Results.Ok(locations);
        })
        .WithName("GetTripLocations")
        .WithOpenApi();

        // Add trip rating
        group.MapPost("/{id}/ratings", async (
            Guid id, 
            CreateTripRatingDto ratingDto, 
            ITripService tripService) =>
        {
            var result = await tripService.AddTripRatingAsync(id, ratingDto);
            if (result == null)
                return Results.NotFound();

            return Results.Created($"/api/trips/{id}/ratings", result);
        })
        .WithName("AddTripRating")
        .WithOpenApi();

        // Get trip rating
        group.MapGet("/{id}/ratings", async (Guid id, ITripService tripService) =>
        {
            var rating = await tripService.GetTripRatingAsync(id);
            if (rating == null)
                return Results.NotFound();
                
            return Results.Ok(rating);
        })
        .WithName("GetTripRating")
        .WithOpenApi();

        // Add trip payment
        group.MapPost("/{id}/payments", async (
            Guid id, 
            CreateTripPaymentDto paymentDto, 
            ITripService tripService) =>
        {
            var result = await tripService.AddTripPaymentAsync(id, paymentDto);
            if (result == null)
                return Results.NotFound();

            return Results.Created($"/api/trips/{id}/payments", result);
        })
        .WithName("AddTripPayment")
        .WithOpenApi();

        // Get trip payment
        group.MapGet("/{id}/payments", async (Guid id, ITripService tripService) =>
        {
            var payment = await tripService.GetTripPaymentAsync(id);
            if (payment == null)
                return Results.NotFound();
                
            return Results.Ok(payment);
        })
        .WithName("GetTripPayment")
        .WithOpenApi();

        // Update payment status
        group.MapPatch("/{id}/payments", async (
            Guid id, 
            bool isPaid, 
            string? paymentReference, 
            ITripService tripService) =>
        {
            var result = await tripService.UpdateTripPaymentStatusAsync(id, isPaid, paymentReference);
            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("UpdateTripPaymentStatus")
        .WithOpenApi();
    }
} 