using DriveApp.DTOs.Users;
using DriveApp.Models.Users;

namespace DriveApp.Services.Users;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(Guid id);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);
    Task<bool> DeleteUserAsync(Guid id);
    Task<UserDto?> GetUserByPhoneNumberAsync(string phoneNumber);
    Task<IEnumerable<RoleDto>> GetUserRolesAsync(Guid userId);
    Task<bool> AddRoleToUserAsync(Guid userId, Guid roleId);
    Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId);
    Task<bool> HasPermissionAsync(Guid userId, string permissionName);
    Task<UserDto> UpdateUserLastLoginAsync(Guid userId);
    string GenerateJwtToken(UserDto user);

    // Authentication methods
    Task<string> RequestPhoneVerificationAsync(string phoneNumber);
    Task<VerificationResultDto> VerifyPhoneNumberAsync(string phoneNumber, string verificationCode);
    Task<LoginResultDto> LoginAsync(string phoneNumber, string verificationCode);
    Task<TokenResultDto> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string token);
    Task<UserDto> GetUserFromTokenAsync(string token);
} 