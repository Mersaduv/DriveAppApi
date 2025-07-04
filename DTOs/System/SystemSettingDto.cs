namespace DriveApp.DTOs.System;

public class SystemSettingDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsPublic { get; set; }
}

public class CreateSystemSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsPublic { get; set; } = false;
}

public class UpdateSystemSettingDto
{
    public string? Value { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool? IsPublic { get; set; }
} 