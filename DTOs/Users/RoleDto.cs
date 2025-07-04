using DriveApp.Enums;

namespace DriveApp.DTOs.Users;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public UserRoleType RoleType { get; set; }
    public bool IsSystemRole { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public UserRoleType RoleType { get; set; }
    public List<Guid> PermissionIds { get; set; } = new List<Guid>();
}

public class UpdateRoleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<Guid>? PermissionIds { get; set; }
} 