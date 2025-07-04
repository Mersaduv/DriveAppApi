namespace DriveApp.Enums;

public enum DriverStatus
{
    Pending = 1,        // در انتظار تایید
    Approved = 2,       // تایید شده
    Rejected = 3,       // رد شده
    Suspended = 4,      // تعلیق
    Active = 5,         // فعال
    Offline = 6         // آفلاین
} 