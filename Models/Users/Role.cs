using DriveApp.Models.Core;
using DriveApp.Enums;

namespace DriveApp.Models.Users;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public UserRoleType RoleType { get; set; }
    public bool IsSystemRole { get; set; } = false;
    
    // Navigation Properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
} 