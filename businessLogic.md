*Thinking...*

> کاربر میخواد یک پروژه مشابه اسنپ و تپسی راه‌اندازی کنه. از نظر معماری:
> - Backend: .NET 9 Minimal API با PostgreSQL
> - Frontend: KMP (Kotlin Multiplatform)
> - WebSocket پشتیبانی لازمه
> 
> نیازهای اصلی:
> 1. مدیریت نقش و دسترسی (Role-based permissions)
> 2. ثبت‌نام راننده با مدارک
> 3. ثبت‌نام مسافر ساده
> 4. مدیریت وسیله نقلیه
> 5. مدیریت سفرها
> 6. تاریخچه سفرها
> 
> Models که باید تعریف کنم:
> 
> 1. User (کاربر اصلی)
> 2. Role (نقش)
> 3. Permission (دسترسی)
> 4. UserRole (رابطه کاربر-نقش)
> 5. RolePermission (رابطه نقش-دسترسی)
> 6. Driver (راننده)
> 7. Passenger (مسافر)
> 8. Vehicle (وسیله نقلیه)
> 9. VehicleType (نوع وسیله)
> 10. Trip (سفر)
> 11. TripStatus (وضعیت سفر)
> 12. Location (موقعیت)
> 13. Document (مدارک)
> 
> همچنین باید:
> - Entity configurations برای EF Core
> - Enums برای وضعیت‌های مختلف
> - DTOs برای API
> - WebSocket models
> 
> بیاید شروع کنم:

بر اساس نیازهای شما، یک architecture کامل برای backend پروژه ride-sharing شما طراحی می‌کنم. در ادامه تمام models و business logic مورد نیاز را تعریف می‌کنم:

## 1. Enums و Constants

```csharp
// Enums/UserRoles.cs
public enum UserRoleType
{
    SuperAdmin = 1,
    Admin = 2,
    DriverRegistrar = 3,
    Driver = 4,
    Passenger = 5
}

// Enums/TripStatus.cs
public enum TripStatus
{
    Requested = 1,      // درخواست شده
    Accepted = 2,       // پذیرفته شده
    DriverArrived = 3,  // راننده رسیده
    InProgress = 4,     // در حال انجام
    Completed = 5,      // تکمیل شده
    Cancelled = 6,      // لغو شده
    Failed = 7          // ناموفق
}

// Enums/VehicleTypes.cs
public enum VehicleType
{
    NormalCar = 1,      // ماشین معمولی
    LuxuryVehicle = 2,  // وسیله لوکس
    Rickshaw = 3,       // ریکشا (سه چرخ)
    Motorcycle = 4,     // موتور
    Taxi = 5,           // تاکسی
    Van = 6             // ون
}

// Enums/DocumentType.cs
public enum DocumentType
{
    NationalCard = 1,           // شناسنامه
    DrivingLicense = 2,         // گواهینامه
    DriverPhoto = 3,            // عکس راننده
    VehicleFront = 4,           // جلو ماشین
    VehicleBack = 5,            // عقب ماشین
    VehicleSide = 6,            // بغل ماشین
    VehicleDocuments = 7        // مدارک ماشین
}

// Enums/DriverStatus.cs
public enum DriverStatus
{
    Pending = 1,        // در انتظار تایید
    Approved = 2,       // تایید شده
    Rejected = 3,       // رد شده
    Suspended = 4,      // تعلیق
    Active = 5,         // فعال
    Offline = 6         // آفلاین
}

// Enums/Permissions.cs
public enum PermissionType
{
    // User Management
    ViewUsers = 1,
    CreateUser = 2,
    EditUser = 3,
    DeleteUser = 4,
    
    // Driver Management
    ViewDrivers = 5,
    ApproveDriver = 6,
    RejectDriver = 7,
    SuspendDriver = 8,
    
    // Trip Management
    ViewTrips = 9,
    ManageTrips = 10,
    ViewTripReports = 11,
    
    // System Settings
    ManageRoles = 12,
    ManagePermissions = 13,
    SystemSettings = 14
}
```

## 2. Core Models

```csharp
// Models/Core/BaseEntity.cs
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

// Models/Users/User.cs
public class User : BaseEntity
{
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsPhoneVerified { get; set; } = false;
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

// Models/Users/Role.cs
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

// Models/Users/Permission.cs
public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PermissionType PermissionType { get; set; }
    public string? Category { get; set; }
    
    // Navigation Properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

// Models/Users/UserRole.cs
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

// Models/Users/RolePermission.cs
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    
    // Navigation Properties
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}

// Models/Users/PhoneVerification.cs
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
```

## 3. Driver Models

```csharp
// Models/Drivers/Driver.cs
public class Driver : BaseEntity
{
    public Guid UserId { get; set; }
    public string? NationalCardNumber { get; set; }
    public string FullAddress { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DriverStatus Status { get; set; } = DriverStatus.Pending;
    public decimal Rating { get; set; } = 0;
    public int TotalTrips { get; set; } = 0;
    public bool IsOnline { get; set; } = false;
    public DateTime? LastLocationUpdate { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public virtual ICollection<DriverLocation> LocationHistory { get; set; } = new List<DriverLocation>();
}

// Models/Drivers/Vehicle.cs
public class Vehicle : BaseEntity
{
    public Guid DriverId { get; set; }
    public VehicleType VehicleType { get; set; }
    public string Make { get; set; } = string.Empty;         // نام خودرو (پژو، پراید و...)
    public string Model { get; set; } = string.Empty;        // مدل خودرو
    public string? Year { get; set; }
    public string? Color { get; set; }
    public string? PlateNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } = false;
    
    // Navigation Properties
    public virtual Driver Driver { get; set; } = null!;
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}

// Models/Drivers/Document.cs
public class Document : BaseEntity
{
    public Guid? DriverId { get; set; }
    public Guid? VehicleId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    public string? RejectionReason { get; set; }
    
    // Navigation Properties
    public virtual Driver? Driver { get; set; }
    public virtual Vehicle? Vehicle { get; set; }
}

// Models/Drivers/DriverLocation.cs
public class DriverLocation : BaseEntity
{
    public Guid DriverId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double? Speed { get; set; }
    public double? Heading { get; set; }
    
    // Navigation Properties
    public virtual Driver Driver { get; set; } = null!;
}
```

## 4. Passenger Models

```csharp
// Models/Passengers/Passenger.cs
public class Passenger : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Rating { get; set; } = 0;
    public int TotalTrips { get; set; } = 0;
    public string? PreferredPaymentMethod { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public virtual ICollection<PassengerFavoriteLocation> FavoriteLocations { get; set; } = new List<PassengerFavoriteLocation>();
}

// Models/Passengers/PassengerFavoriteLocation.cs
public class PassengerFavoriteLocation : BaseEntity
{
    public Guid PassengerId { get; set; }
    public string Name { get; set; } = string.Empty; // خانه، محل کار و...
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    // Navigation Properties
    public virtual Passenger Passenger { get; set; } = null!;
}
```

## 5. Trip Models

```csharp
// Models/Trips/Trip.cs
public class Trip : BaseEntity
{
    public Guid PassengerId { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? VehicleId { get; set; }
    public string TripCode { get; set; } = string.Empty; // کد یکتا سفر
    
    // Origin Information
    public string OriginAddress { get; set; } = string.Empty;
    public double OriginLatitude { get; set; }
    public double OriginLongitude { get; set; }
    
    // Destination Information
    public string DestinationAddress { get; set; } = string.Empty;
    public double DestinationLatitude { get; set; }
    public double DestinationLongitude { get; set; }
    
    // Trip Details
    public TripStatus Status { get; set; } = TripStatus.Requested;
    public VehicleType RequestedVehicleType { get; set; }
    public decimal EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal? Distance { get; set; } // به کیلومتر
    public int? EstimatedDuration { get; set; } // به دقیقه
    public int? ActualDuration { get; set; } // به دقیقه
    
    // Timing
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DriverArrivedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    // Additional Info
    public string? PassengerNotes { get; set; }
    public string? CancellationReason { get; set; }
    public string? CancelledBy { get; set; } // Driver or Passenger
    
    // Navigation Properties
    public virtual Passenger Passenger { get; set; } = null!;
    public virtual Driver? Driver { get; set; }
    public virtual Vehicle? Vehicle { get; set; }
    public virtual ICollection<TripLocation> TripLocations { get; set; } = new List<TripLocation>();
    public virtual TripRating? Rating { get; set; }
    public virtual TripPayment? Payment { get; set; }
}

// Models/Trips/TripLocation.cs
public class TripLocation : BaseEntity
{
    public Guid TripId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double? Speed { get; set; }
    public double? Heading { get; set; }
    
    // Navigation Properties
    public virtual Trip Trip { get; set; } = null!;
}

// Models/Trips/TripRating.cs
public class TripRating : BaseEntity
{
    public Guid TripId { get; set; }
    public int PassengerRating { get; set; } // 1-5
    public int? DriverRating { get; set; } // 1-5
    public string? PassengerComment { get; set; }
    public string? DriverComment { get; set; }
    public DateTime? RatedAt { get; set; }
    
    // Navigation Properties
    public virtual Trip Trip { get; set; } = null!;
}

// Models/Trips/TripPayment.cs
public class TripPayment : BaseEntity
{
    public Guid TripId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, Wallet
    public string? PaymentReference { get; set; }
    public bool IsPaid { get; set; } = false;
    public DateTime? PaidAt { get; set; }
    
    // Navigation Properties
    public virtual Trip Trip { get; set; } = null!;
}
```

## 6. WebSocket Models

```csharp
// Models/WebSocket/WebSocketMessage.cs
public class WebSocketMessage
{
    public string Type { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? RequestId { get; set; }
}

// Models/WebSocket/LocationUpdate.cs
public class LocationUpdate
{
    public Guid UserId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Speed { get; set; }
    public double? Heading { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Models/WebSocket/TripUpdate.cs
public class TripUpdate
{
    public Guid TripId { get; set; }
    public TripStatus Status { get; set; }
    public string? Message { get; set; }
    public object? AdditionalData { get; set; }
}

// Models/WebSocket/DriverStatusUpdate.cs
public class DriverStatusUpdate
{
    public Guid DriverId { get; set; }
    public bool IsOnline { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
```

## 7. System Configuration Models

```csharp
// Models/System/SystemSetting.cs
public class SystemSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsPublic { get; set; } = false; // آیا در API عمومی در دسترس باشد
}

// Models/System/PriceConfiguration.cs
public class PriceConfiguration : BaseEntity
{
    public VehicleType VehicleType { get; set; }
    public decimal BasePrice { get; set; }        // قیمت پایه
    public decimal PricePerKm { get; set; }       // قیمت هر کیلومتر
    public decimal PricePerMinute { get; set; }   // قیمت هر دقیقه
    public decimal MinimumPrice { get; set; }     // حداقل قیمت
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}
```

این مدل‌ها تمام نیازهای اصلی پروژه شما را پوشش می‌دهند و امکان مدیریت کامل سیستم ride-sharing را فراهم می‌کنند. آیا نیاز به توضیح بیشتر درباره هر بخش یا اضافه کردن مدل‌های دیگری دارید؟