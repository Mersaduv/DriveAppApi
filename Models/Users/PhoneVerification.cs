using DriveApp.Models.Core;

namespace DriveApp.Models.Users;

public class PhoneVerification : BaseEntity
{
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
} 