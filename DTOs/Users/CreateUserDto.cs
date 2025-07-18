using System.ComponentModel.DataAnnotations;
using DriveApp.Models.Users;

namespace DriveApp.DTOs.Users;

public class CreateUserDto
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public string? FullName { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    public List<Guid>? RoleIds { get; set; }
    
    public User ToEntity()
    {
        return new User
        {
            PhoneNumber = PhoneNumber,
            FullName = FullName,
            Email = Email,
            DateOfBirth = DateOfBirth,
            IsPhoneVerified = true, // Phone is verified through Firebase
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
} 