using DriveApp.DTOs.Users;

namespace DriveApp.Services.Users;

public interface IRoleService
{
    Task<RoleDto> GetRoleByIdAsync(Guid id);
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto);
    Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleDto updateRoleDto);
    Task<bool> DeleteRoleAsync(Guid id);
    Task<bool> AddPermissionToRoleAsync(Guid roleId, Guid permissionId);
    Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId);
} 