using DriveApp.Models.Core;

namespace DriveApp.Models.System;

public class SystemSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsPublic { get; set; } = false; // Available in public API
} 