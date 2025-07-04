using DriveApp.DTOs.System;
using DriveApp.Services.System;

namespace DriveApp.Endpoints.System;

public static class SystemSettingsEndpoints
{
    public static void MapSystemSettingsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/settings")
            .WithTags("System Settings");
            
        // Get all settings
        group.MapGet("/", async (ISystemSettingService settingService) =>
        {
            var settings = await settingService.GetAllSettingsAsync();
            return Results.Ok(settings);
        })
        .WithName("GetAllSettings")
        .WithDescription("Gets all system settings")
        .Produces<IEnumerable<SystemSettingDto>>(200);
        
        // Get public settings only
        group.MapGet("/public", async (ISystemSettingService settingService) =>
        {
            var settings = await settingService.GetPublicSettingsAsync();
            return Results.Ok(settings);
        })
        .WithName("GetPublicSettings")
        .WithDescription("Gets all public system settings")
        .Produces<IEnumerable<SystemSettingDto>>(200);
        
        // Get settings by category
        group.MapGet("/category/{category}", async (string category, ISystemSettingService settingService) =>
        {
            var settings = await settingService.GetSettingsByCategoryAsync(category);
            return Results.Ok(settings);
        })
        .WithName("GetSettingsByCategory")
        .WithDescription("Gets system settings by category")
        .Produces<IEnumerable<SystemSettingDto>>(200);
        
        // Get setting by ID
        group.MapGet("/{id}", async (Guid id, ISystemSettingService settingService) =>
        {
            var setting = await settingService.GetSettingByIdAsync(id);
            if (setting == null)
                return Results.NotFound();
                
            return Results.Ok(setting);
        })
        .WithName("GetSettingById")
        .WithDescription("Gets a system setting by ID")
        .Produces<SystemSettingDto>(200)
        .Produces(404);
        
        // Get setting by key
        group.MapGet("/key/{key}", async (string key, ISystemSettingService settingService) =>
        {
            var setting = await settingService.GetSettingByKeyAsync(key);
            if (setting == null)
                return Results.NotFound();
                
            return Results.Ok(setting);
        })
        .WithName("GetSettingByKey")
        .WithDescription("Gets a system setting by key")
        .Produces<SystemSettingDto>(200)
        .Produces(404);
        
        // Create setting
        group.MapPost("/", async (CreateSystemSettingDto settingDto, ISystemSettingService settingService) =>
        {
            try
            {
                var createdSetting = await settingService.CreateSettingAsync(settingDto);
                return Results.Created($"/api/settings/{createdSetting.Id}", createdSetting);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("CreateSetting")
        .WithDescription("Creates a new system setting")
        .Produces<SystemSettingDto>(201)
        .Produces(400);
        
        // Update setting
        group.MapPut("/{id}", async (Guid id, UpdateSystemSettingDto settingDto, ISystemSettingService settingService) =>
        {
            var updatedSetting = await settingService.UpdateSettingAsync(id, settingDto);
            if (updatedSetting == null)
                return Results.NotFound();
                
            return Results.Ok(updatedSetting);
        })
        .WithName("UpdateSetting")
        .WithDescription("Updates an existing system setting")
        .Produces<SystemSettingDto>(200)
        .Produces(404);
        
        // Delete setting
        group.MapDelete("/{id}", async (Guid id, ISystemSettingService settingService) =>
        {
            var result = await settingService.DeleteSettingAsync(id);
            if (!result)
                return Results.NotFound();
                
            return Results.NoContent();
        })
        .WithName("DeleteSetting")
        .WithDescription("Deletes a system setting")
        .Produces(204)
        .Produces(404);
    }
} 