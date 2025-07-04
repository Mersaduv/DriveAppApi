using DriveApp.Models.Core;

namespace DriveApp.Models.Users;

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? AssignedBy { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
} 