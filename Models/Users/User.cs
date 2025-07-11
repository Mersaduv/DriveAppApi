using DriveApp.Models.Core;
using DriveApp.Models.Drivers;
using DriveApp.Models.Passengers;

namespace DriveApp.Models.Users;

public class User : BaseEntity
{
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsPhoneVerified { get; set; } = false;
    public DateTime? PhoneVerifiedAt { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation Properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual Driver? Driver { get; set; }
    public virtual Passenger? Passenger { get; set; }
    public virtual ICollection<PhoneVerification> PhoneVerifications { get; set; } = new List<PhoneVerification>();
} 