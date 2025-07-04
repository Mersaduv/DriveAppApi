using DriveApp.DTOs.Users;
using DriveApp.Services.Users;

namespace DriveApp.Endpoints.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/users")
            .WithTags("Users");
            
        // Get all users
        group.MapGet("/", async (IUserService userService) =>
        {
            var users = await userService.GetAllUsersAsync();
            return Results.Ok(users);
        })
        .WithName("GetAllUsers")
        .WithDescription("Gets all users")
        .Produces<IEnumerable<UserDto>>(200);
        
        // Get user by ID
        group.MapGet("/{id}", async (Guid id, IUserService userService) =>
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user == null)
                return Results.NotFound();
                
            return Results.Ok(user);
        })
        .WithName("GetUserById")
        .WithDescription("Gets a user by ID")
        .Produces<UserDto>(200)
        .Produces(404);
        
        // Create user
        group.MapPost("/", async (CreateUserDto userDto, IUserService userService) =>
        {
            var createdUser = await userService.CreateUserAsync(userDto);
            return Results.Created($"/api/users/{createdUser.Id}", createdUser);
        })
        .WithName("CreateUser")
        .WithDescription("Creates a new user")
        .Produces<UserDto>(201);
        
        // Update user
        group.MapPut("/{id}", async (Guid id, UpdateUserDto userDto, IUserService userService) =>
        {
            var updatedUser = await userService.UpdateUserAsync(id, userDto);
            if (updatedUser == null)
                return Results.NotFound();
                
            return Results.Ok(updatedUser);
        })
        .WithName("UpdateUser")
        .WithDescription("Updates an existing user")
        .Produces<UserDto>(200)
        .Produces(404);
        
        // Delete user
        group.MapDelete("/{id}", async (Guid id, IUserService userService) =>
        {
            var result = await userService.DeleteUserAsync(id);
            if (!result)
                return Results.NotFound();
                
            return Results.NoContent();
        })
        .WithName("DeleteUser")
        .WithDescription("Deletes a user")
        .Produces(204)
        .Produces(404);
        
        // Get user roles
        group.MapGet("/{id}/roles", async (Guid id, IUserService userService) =>
        {
            var roles = await userService.GetUserRolesAsync(id);
            return Results.Ok(roles);
        })
        .WithName("GetUserRoles")
        .WithDescription("Gets the roles assigned to a user")
        .Produces<IEnumerable<RoleDto>>(200)
        .Produces(404);
        
        // Add role to user
        group.MapPost("/{userId}/roles/{roleId}", async (Guid userId, Guid roleId, IUserService userService) =>
        {
            var result = await userService.AddRoleToUserAsync(userId, roleId);
            if (!result)
                return Results.BadRequest();
                
            return Results.NoContent();
        })
        .WithName("AddRoleToUser")
        .WithDescription("Adds a role to a user")
        .Produces(204)
        .Produces(400);
        
        // Remove role from user
        group.MapDelete("/{userId}/roles/{roleId}", async (Guid userId, Guid roleId, IUserService userService) =>
        {
            var result = await userService.RemoveRoleFromUserAsync(userId, roleId);
            if (!result)
                return Results.BadRequest();
                
            return Results.NoContent();
        })
        .WithName("RemoveRoleFromUser")
        .WithDescription("Removes a role from a user")
        .Produces(204)
        .Produces(400);
    }
} 