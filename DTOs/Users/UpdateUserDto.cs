using System.ComponentModel.DataAnnotations;
using DriveApp.Models.Users;

namespace DriveApp.DTOs.Users;

public class UpdateUserDto
{
    public string? FullName { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    public bool? IsActive { get; set; }
    
    public List<Guid>? RoleIds { get; set; }
}

public static class UserDtoExtensionsForUpdate
{
    public static void UpdateFromDto(this User user, UpdateUserDto dto)
    {
        if (dto.FullName != null)
            user.FullName = dto.FullName;
            
        if (dto.Email != null)
            user.Email = dto.Email;
            
        if (dto.DateOfBirth.HasValue)
            user.DateOfBirth = dto.DateOfBirth;
            
        if (dto.IsActive.HasValue)
            user.IsActive = dto.IsActive.Value;
            
        user.UpdatedAt = DateTime.UtcNow;
    }
} 