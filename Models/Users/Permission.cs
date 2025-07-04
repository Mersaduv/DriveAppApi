using DriveApp.Models.Core;
using DriveApp.Enums;

namespace DriveApp.Models.Users;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PermissionType PermissionType { get; set; }
    public string? Category { get; set; }
    
    // Navigation Properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
} 