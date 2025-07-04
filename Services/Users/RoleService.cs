using Microsoft.EntityFrameworkCore;
using DriveApp.Data;
using DriveApp.DTOs.Users;
using DriveApp.Models.Users;
using DriveApp.Services.Helpers;

namespace DriveApp.Services.Users;

public class RoleService : BaseService, IRoleService
{
    public RoleService(AppDbContext dbContext, ILogger<RoleService> logger)
        : base(dbContext, logger)
    {
    }
    
    public async Task<RoleDto> GetRoleByIdAsync(Guid id)
    {
        var role = await _dbContext.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id);
            
        if (role == null)
            return null;
            
        return role.ToDto();
    }
    
    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _dbContext.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .ToListAsync();
            
        return roles.Select(r => r.ToDto());
    }
    
    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto)
    {
        // Check if role name already exists
        var existingRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == createRoleDto.Name);
        if (existingRole != null)
            throw new InvalidOperationException($"Role with name {createRoleDto.Name} already exists");
            
        var role = createRoleDto.ToEntity();
        
        await _dbContext.Roles.AddAsync(role);
        
        // Add permissions if provided
        if (createRoleDto.PermissionIds != null && createRoleDto.PermissionIds.Any())
        {
            foreach (var permissionId in createRoleDto.PermissionIds)
            {
                var permission = await _dbContext.Permissions.FindAsync(permissionId);
                if (permission != null)
                {
                    await _dbContext.RolePermissions.AddAsync(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id
                    });
                }
            }
        }
        
        await _dbContext.SaveChangesAsync();
        
        return await GetRoleByIdAsync(role.Id);
    }
    
    public async Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleDto updateRoleDto)
    {
        var role = await _dbContext.Roles.FindAsync(id);
        if (role == null)
            return null;
            
        // Check if system role
        if (role.IsSystemRole)
            throw new InvalidOperationException("System roles cannot be modified");
            
        role.UpdateFromDto(updateRoleDto);
        
        // Update permissions if provided
        if (updateRoleDto.PermissionIds != null)
        {
            // Get current permissions
            var currentPermissions = await _dbContext.RolePermissions
                .Where(rp => rp.RoleId == id)
                .ToListAsync();
            
            // Remove permissions that are not in the new list
            foreach (var rolePermission in currentPermissions)
            {
                if (!updateRoleDto.PermissionIds.Contains(rolePermission.PermissionId))
                {
                    _dbContext.RolePermissions.Remove(rolePermission);
                }
            }
            
            // Add new permissions
            foreach (var permissionId in updateRoleDto.PermissionIds)
            {
                if (!currentPermissions.Any(rp => rp.PermissionId == permissionId))
                {
                    var permission = await _dbContext.Permissions.FindAsync(permissionId);
                    if (permission != null)
                    {
                        await _dbContext.RolePermissions.AddAsync(new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = permission.Id
                        });
                    }
                }
            }
        }
        
        _dbContext.Roles.Update(role);
        await _dbContext.SaveChangesAsync();
        
        return await GetRoleByIdAsync(id);
    }
    
    public async Task<bool> DeleteRoleAsync(Guid id)
    {
        var role = await _dbContext.Roles.FindAsync(id);
        if (role == null)
            return false;
            
        // Check if system role
        if (role.IsSystemRole)
            throw new InvalidOperationException("System roles cannot be deleted");
            
        // Check if role is assigned to any user
        var usersWithRole = await _dbContext.UserRoles.AnyAsync(ur => ur.RoleId == id);
        if (usersWithRole)
            throw new InvalidOperationException("Cannot delete a role that is assigned to users");
            
        // Soft delete
        role.IsDeleted = true;
        role.UpdatedAt = DateTime.UtcNow;
        
        _dbContext.Roles.Update(role);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> AddPermissionToRoleAsync(Guid roleId, Guid permissionId)
    {
        var role = await _dbContext.Roles.FindAsync(roleId);
        if (role == null)
            return false;
            
        var permission = await _dbContext.Permissions.FindAsync(permissionId);
        if (permission == null)
            return false;
            
        // Check if role already has this permission
        var existingRolePermission = await _dbContext.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
            
        if (existingRolePermission != null)
            return true; // Role already has this permission
            
        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
        
        await _dbContext.RolePermissions.AddAsync(rolePermission);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId)
    {
        var rolePermission = await _dbContext.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
            
        if (rolePermission == null)
            return false;
            
        _dbContext.RolePermissions.Remove(rolePermission);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
} 