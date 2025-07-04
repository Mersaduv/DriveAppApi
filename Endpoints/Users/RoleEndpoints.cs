using DriveApp.DTOs.Users;
using DriveApp.Services.Users;

namespace DriveApp.Endpoints.Users;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/roles")
            .WithTags("Roles");
            
        // Get all roles
        group.MapGet("/", async (IRoleService roleService) =>
        {
            var roles = await roleService.GetAllRolesAsync();
            return Results.Ok(roles);
        })
        .WithName("GetAllRoles")
        .WithDescription("Gets all roles")
        .Produces<IEnumerable<RoleDto>>(200);
        
        // Get role by ID
        group.MapGet("/{id}", async (Guid id, IRoleService roleService) =>
        {
            var role = await roleService.GetRoleByIdAsync(id);
            if (role == null)
                return Results.NotFound();
                
            return Results.Ok(role);
        })
        .WithName("GetRoleById")
        .WithDescription("Gets a role by ID")
        .Produces<RoleDto>(200)
        .Produces(404);
        
        // Create role
        group.MapPost("/", async (CreateRoleDto roleDto, IRoleService roleService) =>
        {
            var createdRole = await roleService.CreateRoleAsync(roleDto);
            return Results.Created($"/api/roles/{createdRole.Id}", createdRole);
        })
        .WithName("CreateRole")
        .WithDescription("Creates a new role")
        .Produces<RoleDto>(201);
        
        // Update role
        group.MapPut("/{id}", async (Guid id, UpdateRoleDto roleDto, IRoleService roleService) =>
        {
            var updatedRole = await roleService.UpdateRoleAsync(id, roleDto);
            if (updatedRole == null)
                return Results.NotFound();
                
            return Results.Ok(updatedRole);
        })
        .WithName("UpdateRole")
        .WithDescription("Updates an existing role")
        .Produces<RoleDto>(200)
        .Produces(404);
        
        // Delete role
        group.MapDelete("/{id}", async (Guid id, IRoleService roleService) =>
        {
            var result = await roleService.DeleteRoleAsync(id);
            if (!result)
                return Results.NotFound();
                
            return Results.NoContent();
        })
        .WithName("DeleteRole")
        .WithDescription("Deletes a role")
        .Produces(204)
        .Produces(404);
        
        // Add permission to role
        group.MapPost("/{roleId}/permissions/{permissionId}", async (Guid roleId, Guid permissionId, IRoleService roleService) =>
        {
            var result = await roleService.AddPermissionToRoleAsync(roleId, permissionId);
            if (!result)
                return Results.BadRequest();
                
            return Results.NoContent();
        })
        .WithName("AddPermissionToRole")
        .WithDescription("Adds a permission to a role")
        .Produces(204)
        .Produces(400);
        
        // Remove permission from role
        group.MapDelete("/{roleId}/permissions/{permissionId}", async (Guid roleId, Guid permissionId, IRoleService roleService) =>
        {
            var result = await roleService.RemovePermissionFromRoleAsync(roleId, permissionId);
            if (!result)
                return Results.BadRequest();
                
            return Results.NoContent();
        })
        .WithName("RemovePermissionFromRole")
        .WithDescription("Removes a permission from a role")
        .Produces(204)
        .Produces(400);
    }
} 