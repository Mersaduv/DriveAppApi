using DriveApp.DTOs.System;
using DriveApp.Services.System;
using DriveApp.Enums;

namespace DriveApp.Endpoints.System;

public static class PriceConfigurationEndpoints
{
    public static void MapPriceConfigurationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/price-configurations")
            .WithTags("Price Configurations");
            
        // Get all price configurations
        group.MapGet("/", async (IPriceConfigurationService priceService) =>
        {
            var configs = await priceService.GetAllPriceConfigurationsAsync();
            return Results.Ok(configs);
        })
        .WithName("GetAllPriceConfigurations")
        .WithDescription("Gets all price configurations")
        .Produces<IEnumerable<PriceConfigurationDto>>(200);
        
        // Get price configuration by ID
        group.MapGet("/{id}", async (Guid id, IPriceConfigurationService priceService) =>
        {
            var config = await priceService.GetPriceConfigurationByIdAsync(id);
            if (config == null)
                return Results.NotFound();
                
            return Results.Ok(config);
        })
        .WithName("GetPriceConfigurationById")
        .WithDescription("Gets a price configuration by ID")
        .Produces<PriceConfigurationDto>(200)
        .Produces(404);
        
        // Get price configuration by vehicle type
        group.MapGet("/vehicle-type/{vehicleType}", async (VehicleType vehicleType, IPriceConfigurationService priceService) =>
        {
            var config = await priceService.GetPriceConfigurationByVehicleTypeAsync(vehicleType);
            if (config == null)
                return Results.NotFound();
                
            return Results.Ok(config);
        })
        .WithName("GetPriceConfigurationByVehicleType")
        .WithDescription("Gets a price configuration by vehicle type")
        .Produces<PriceConfigurationDto>(200)
        .Produces(404);
        
        // Calculate trip price
        group.MapGet("/calculate-price", async (
            [AsParameters] CalculatePriceRequest request,
            IPriceConfigurationService priceService) =>
        {
            try
            {
                var price = await priceService.CalculateTripPriceAsync(request.VehicleType, request.Distance, request.DurationMinutes);
                return Results.Ok(new { Price = price });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("CalculateTripPrice")
        .WithDescription("Calculates the price for a trip based on vehicle type, distance, and duration")
        .Produces<object>(200)
        .Produces(400);
        
        // Create price configuration
        group.MapPost("/", async (CreatePriceConfigurationDto configDto, IPriceConfigurationService priceService) =>
        {
            try
            {
                var createdConfig = await priceService.CreatePriceConfigurationAsync(configDto);
                return Results.Created($"/api/price-configurations/{createdConfig.Id}", createdConfig);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("CreatePriceConfiguration")
        .WithDescription("Creates a new price configuration")
        .Produces<PriceConfigurationDto>(201)
        .Produces(400);
        
        // Update price configuration
        group.MapPut("/{id}", async (Guid id, UpdatePriceConfigurationDto configDto, IPriceConfigurationService priceService) =>
        {
            var updatedConfig = await priceService.UpdatePriceConfigurationAsync(id, configDto);
            if (updatedConfig == null)
                return Results.NotFound();
                
            return Results.Ok(updatedConfig);
        })
        .WithName("UpdatePriceConfiguration")
        .WithDescription("Updates an existing price configuration")
        .Produces<PriceConfigurationDto>(200)
        .Produces(404);
        
        // Delete price configuration
        group.MapDelete("/{id}", async (Guid id, IPriceConfigurationService priceService) =>
        {
            var result = await priceService.DeletePriceConfigurationAsync(id);
            if (!result)
                return Results.NotFound();
                
            return Results.NoContent();
        })
        .WithName("DeletePriceConfiguration")
        .WithDescription("Deletes a price configuration")
        .Produces(204)
        .Produces(404);
    }
}

// Request model for calculate price endpoint
public class CalculatePriceRequest
{
    public VehicleType VehicleType { get; set; }
    public decimal Distance { get; set; }
    public int DurationMinutes { get; set; }
} 